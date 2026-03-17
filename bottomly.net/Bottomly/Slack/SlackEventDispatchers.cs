using Bottomly.Slack.MembershipEventHandlers;
using SlackNet;
using SlackNet.Events;

namespace Bottomly.Slack;

/// <summary>
///     Bridges SlackNet's IEventHandler&lt;MessageEvent&gt; to our SlackWorker message processing pipeline.
/// </summary>
public class SlackMessageEventDispatcher(SlackWorker worker) : IEventHandler<MessageEvent>
{
    public Task Handle(MessageEvent slackEvent) => worker.ProcessMessageAsync(slackEvent);
}

/// <summary>
///     Bridges SlackNet's IEventHandler&lt;ReactionAdded&gt; to our SlackWorker reaction processing pipeline.
/// </summary>
public class SlackReactionEventDispatcher(SlackWorker worker) : IEventHandler<ReactionAdded>
{
    public Task Handle(ReactionAdded slackEvent) => worker.ProcessReactionAsync(slackEvent);
}

public class SlackMemberAddedEventDispatcher(MemberJoinedEventHandler handler)
    : IEventHandler<MemberJoinedChannel>
{
    public async Task Handle(MemberJoinedChannel slackEvent) => await handler.ExecuteAsync(slackEvent);
}