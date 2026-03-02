using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.Events;

namespace Bottomly.Slack;

/// <summary>
///     Bridges SlackNet's IEventHandler&lt;MessageEvent&gt; to our SlackWorker message processing pipeline.
/// </summary>
public class SlackMessageEventDispatcher(SlackWorker worker, ILogger<SlackMessageEventDispatcher> logger)
    : IEventHandler<MessageEvent>
{
    public async Task Handle(MessageEvent slackEvent)
    {
        logger.LogDebug("Received message event: {Event}", slackEvent);
        await worker.ProcessMessageAsync(slackEvent);
    }
}

/// <summary>
///     Bridges SlackNet's IEventHandler&lt;ReactionAdded&gt; to our SlackWorker reaction processing pipeline.
/// </summary>
public class SlackReactionEventDispatcher(SlackWorker worker, ILogger<SlackReactionEventDispatcher> logger)
    : IEventHandler<ReactionAdded>
{
    public async Task Handle(ReactionAdded slackEvent)
    {
        logger.LogDebug("Received reaction event: {Event}", slackEvent);
        await worker.ProcessReactionAsync(slackEvent);
    }
}