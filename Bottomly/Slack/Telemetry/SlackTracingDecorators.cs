using System.Diagnostics;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Slack.ReactionHandlers;
using Bottomly.Telemetry;
using SlackNet.Events;

namespace Bottomly.Slack.Telemetry;

internal sealed class TracingMessageEventHandlerDecorator(IMessageEventHandler inner) : IMessageEventHandler
{
    public bool CanHandle(MessageEvent message) => inner.CanHandle(message);

    public string BuildHelpMessage() => inner.BuildHelpMessage();

    public async Task HandleAsync(MessageEvent message)
    {
        using var activity = BottomlyActivitySource.Instance.StartActivity("slack.handler.invoke");
        activity?.SetTag("slack.handler", inner.GetType().Name);
        activity?.SetTag("slack.text_preview", message.Text?[..Math.Min(message.Text.Length, 100)]);

        try
        {
            await inner.HandleAsync(message);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}

internal sealed class TracingReactionHandlerDecorator(IReactionHandler inner) : IReactionHandler
{
    public bool CanHandle(ReactionAdded reaction) => inner.CanHandle(reaction);

    public async Task HandleAsync(ReactionAdded reaction)
    {
        using var activity = BottomlyActivitySource.Instance.StartActivity("slack.reaction.handler.invoke");
        activity?.SetTag("slack.handler", inner.GetType().Name);
        activity?.SetTag("slack.reaction", reaction.Reaction);

        try
        {
            await inner.HandleAsync(reaction);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
