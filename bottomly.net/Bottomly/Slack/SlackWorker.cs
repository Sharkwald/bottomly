using Bottomly.Repositories;
using Bottomly.Slack.EventHandlers;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Slack.ReactionHandlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.Events;
using SlackNet.SocketMode;

namespace Bottomly.Slack;

public class SlackWorker(
    ISlackSocketModeClient socketClient,
    IEnumerable<IMessageEventHandler> eventHandlers,
    HelpHandler helpMessageHandler,
    IEnumerable<IReactionHandler> reactionHandlers,
    IMemberRepository memberRepository,
    ILogger<SlackWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting Slack Socket Mode connection...");

        await socketClient.Connect(new SocketModeConnectionOptions(), stoppingToken);

        logger.LogInformation("Connected to Slack.");

        // Keep alive until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public async Task ProcessMessageAsync(MessageEvent message)
    {
        try
        {
            if (!IsSubscribedMessage(message))
            {
                return;
            }

            await ResolveUsernameAsync(message);

            // Help handler takes priority
            if (helpMessageHandler.CanHandle(message))
            {
                await helpMessageHandler.HandleAsync(message);
                return;
            }

            foreach (var handler in eventHandlers)
            {
                if (handler.CanHandle(message))
                {
                    await handler.HandleAsync(message);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Slack message: {Text}", message.Text);
        }
    }

    public async Task ProcessReactionAsync(ReactionAdded reaction)
    {
        try
        {
            foreach (var handler in reactionHandlers)
            {
                if (handler.CanHandle(reaction))
                {
                    await handler.HandleAsync(reaction);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing reaction: {Reaction}", reaction.Reaction);
        }
    }

    private static bool IsSubscribedMessage(MessageEvent message) =>
        !string.IsNullOrEmpty(message.Text) && string.IsNullOrEmpty(message.BotId);

    private async Task ResolveUsernameAsync(MessageEvent message)
    {
        try
        {
            var member = await memberRepository.GetBySlackIdAsync(message.User);
            if (member is not null)
            {
                message.User = member.Username;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not resolve username for Slack ID {UserId}", message.User);
        }
    }
}