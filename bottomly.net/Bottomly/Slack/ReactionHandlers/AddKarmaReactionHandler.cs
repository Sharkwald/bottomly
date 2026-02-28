using Bottomly.Commands;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.Events;

namespace Bottomly.Slack.ReactionHandlers;

public class AddKarmaReactionHandler(
    AddKarmaCommand command,
    IMemberRepository memberRepository,
    ISlackMessageBroker broker,
    ILogger<AddKarmaReactionHandler> logger)
    : IReactionHandler
{
    public bool CanHandle(ReactionAdded reactionEvent)
    {
        var reaction = ParseReaction(reactionEvent.Reaction);
        return AddKarmaCommand.GetKarmaReactions().ContainsKey(reaction);
    }

    public async Task HandleAsync(ReactionAdded reactionEvent)
    {
        try
        {
            var reaction = ParseReaction(reactionEvent.Reaction);
            var reactions = AddKarmaCommand.GetKarmaReactions();

            var reactor = await memberRepository.GetBySlackIdAsync(reactionEvent.User);
            var reactee = await memberRepository.GetBySlackIdAsync(reactionEvent.ItemUser);

            if (reactor is null || reactee is null)
            {
                logger.LogWarning("Could not resolve reactor ({ReactorId}) or reactee ({ReacteeId})",
                    reactionEvent.User, reactionEvent.ItemUser);
                return;
            }

            await command.ExecuteAsync(
                reactee.Username,
                reactor.Username,
                $"Reacted with {reaction}",
                reactions[reaction]);

            if (reactionEvent.Item is ReactionMessage messageItem)
            {
                await broker.SendReactionAsync("robot_face", messageItem.Channel, messageItem.Ts);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling reaction {Reaction}", reactionEvent.Reaction);
        }
    }

    private static string ParseReaction(string rawReaction) => rawReaction.Split("::")[0];
}