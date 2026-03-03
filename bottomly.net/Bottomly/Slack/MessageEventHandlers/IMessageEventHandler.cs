using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public interface IMessageEventHandler
{
    bool CanHandle(MessageEvent message);
    Task HandleAsync(MessageEvent message);
    string BuildHelpMessage();
}