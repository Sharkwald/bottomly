using Bottomly.Repositories;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Slack.ReactionHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.Events;
using SlackNet.SocketMode;

namespace Bottomly.Slack;

public class SlackWorker
    : BackgroundService
{
    internal const string HelpHandlerKey = "help";
    internal const string ConversationHandlerKey = "conversation";

    private readonly IMessageEventHandler _conversationHandler;
    private readonly IReadOnlyList<IMessageEventHandler> _handlers;
    private readonly IMessageEventHandler _helpMessageHandler;
    private readonly ILogger<SlackWorker> _logger;
    private readonly IMemberRepository _memberRepository;
    private readonly IEnumerable<IReactionHandler> _reactionHandlers;
    private readonly ISlackSocketModeClient _socketClient;

    public SlackWorker(
        ISlackSocketModeClient socketClient,
        IEnumerable<IMessageEventHandler> eventHandlers,
        [FromKeyedServices(HelpHandlerKey)] IMessageEventHandler helpMessageHandler,
        [FromKeyedServices(ConversationHandlerKey)] IMessageEventHandler conversationHandler,
        IEnumerable<IReactionHandler> reactionHandlers,
        IMemberRepository memberRepository,
        ILogger<SlackWorker> logger)
    {
        _socketClient = socketClient;
        _helpMessageHandler = helpMessageHandler;
        _conversationHandler = conversationHandler;
        _reactionHandlers = reactionHandlers;
        _memberRepository = memberRepository;
        _logger = logger;
        _handlers = eventHandlers.ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Slack Socket Mode connection...");

        await _socketClient.Connect(new SocketModeConnectionOptions(), stoppingToken);

        _logger.LogInformation("Connected to Slack.");

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
            if (_helpMessageHandler.CanHandle(message))
            {
                await _helpMessageHandler.HandleAsync(message);
                return;
            }

            foreach (var handler in _handlers.Where(handler => handler.CanHandle(message)))
            {
                await handler.HandleAsync(message);
                return;
            }

            // Conversation handler is the fallback — only runs if no other handler matched
            if (_conversationHandler.CanHandle(message))
            {
                await _conversationHandler.HandleAsync(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Slack message: {Text}", message.Text);
        }
    }

    public async Task ProcessReactionAsync(ReactionAdded reaction)
    {
        try
        {
            foreach (var handler in _reactionHandlers)
            {
                if (handler.CanHandle(reaction))
                {
                    await handler.HandleAsync(reaction);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reaction: {Reaction}", reaction.Reaction);
        }
    }

    private static bool IsSubscribedMessage(MessageEvent message) =>
        !string.IsNullOrEmpty(message.Text) && string.IsNullOrEmpty(message.BotId);

    private async Task ResolveUsernameAsync(MessageEvent message)
    {
        try
        {
            var member = await _memberRepository.GetBySlackIdAsync(message.User);
            if (member is not null)
            {
                message.User = member.Username;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not resolve username for Slack ID {UserId}", message.User);
        }
    }
}