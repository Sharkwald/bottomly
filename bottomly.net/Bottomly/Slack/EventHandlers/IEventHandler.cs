using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public interface IEventHandler
{
    bool CanHandle(MessageEvent message);
    Task HandleAsync(MessageEvent message);
    string BuildHelpMessage();
}