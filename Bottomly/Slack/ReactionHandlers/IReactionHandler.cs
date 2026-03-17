using SlackNet.Events;

namespace Bottomly.Slack.ReactionHandlers;

public interface IReactionHandler
{
    bool CanHandle(ReactionAdded reactionEvent);
    Task HandleAsync(ReactionAdded reactionEvent);
}