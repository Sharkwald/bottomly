using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.Events;

namespace Bottomly.Slack.MembershipEventHandlers;

public class MemberJoinedEventHandler(
    IMemberRepository repository,
    ISlackApiClient slackClient,
    ILogger<MemberJoinedEventHandler> logger)
{
    public async Task ExecuteAsync(MemberJoinedChannel joinedEvent)
    {
        if (joinedEvent.Channel != "#general")
        {
            return;
        }

        var memberInfo = await slackClient.Users.Info(joinedEvent.User);
        if (memberInfo == null)
        {
            return;
        }

        var member = new Member { SlackId = memberInfo.Id, Username = memberInfo.Name };

        await repository.AddAsync(member);

        logger.LogInformation("Added new member {Username} to the database", member.Username);
    }
}