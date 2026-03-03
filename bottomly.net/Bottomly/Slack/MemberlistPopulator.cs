using Bottomly.Models;
using Bottomly.Repositories;
using SlackNet;

namespace Bottomly.Slack;

public class MemberlistPopulator(ISlackApiClient slack, IMemberRepository memberRepository)
{
    public async Task<IList<Member>> PopulateMembers()
    {
        var users = await slack.Users.List();

        var members = users.Members
            .Where(u => !u.Deleted)
            .Select(u => new Member { SlackId = u.Id, Username = u.Name }).ToList();

        await memberRepository.AddAsync(members);

        return members;
    }
}