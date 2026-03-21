using Bottomly.Slack.MembershipEventHandlers;
using Bottomly.Telemetry;
using SlackNet;
using SlackNet.Events;
using System.Diagnostics;

namespace Bottomly.Slack;

/// <summary>
///     Bridges SlackNet's IEventHandler&lt;MessageEvent&gt; to our SlackWorker message processing pipeline.
/// </summary>
public class SlackMessageEventDispatcher(SlackWorker worker) : IEventHandler<MessageEvent>
{
    public async Task Handle(MessageEvent slackEvent)
    {
        using var activity = BottomlyActivitySource.Instance.StartActivity("slack.message.process");
        activity?.SetTag("slack.channel", slackEvent.Channel);
        activity?.SetTag("slack.user", slackEvent.User);
        activity?.SetTag("slack.text_preview", slackEvent.Text?[..Math.Min(slackEvent.Text.Length, 100)]);

        try
        {
            await worker.ProcessMessageAsync(slackEvent);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}

/// <summary>
///     Bridges SlackNet's IEventHandler&lt;ReactionAdded&gt; to our SlackWorker reaction processing pipeline.
/// </summary>
public class SlackReactionEventDispatcher(SlackWorker worker) : IEventHandler<ReactionAdded>
{
    public async Task Handle(ReactionAdded slackEvent)
    {
        using var activity = BottomlyActivitySource.Instance.StartActivity("slack.reaction.process");
        activity?.SetTag("slack.reaction", slackEvent.Reaction);
        if (slackEvent.Item is ReactionMessage reactionMessage)
            activity?.SetTag("slack.channel", reactionMessage.Channel);

        try
        {
            await worker.ProcessReactionAsync(slackEvent);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}

public class SlackMemberAddedEventDispatcher(MemberJoinedEventHandler handler)
    : IEventHandler<MemberJoinedChannel>
{
    public async Task Handle(MemberJoinedChannel slackEvent) => await handler.ExecuteAsync(slackEvent);
}