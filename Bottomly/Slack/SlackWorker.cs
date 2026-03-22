using Bottomly.Repositories;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;
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

    private readonly List<ConversationMessageHandler> _conversationHandlers;
    private readonly IMessageEventHandler _helpMessageHandler;
    private readonly ILogger<SlackWorker> _logger;
    private readonly IMemberRepository _memberRepository;
    private readonly List<IMessageEventHandler> _nonConversationHandlers;
    private readonly IEnumerable<IReactionHandler> _reactionHandlers;
    private readonly ISlackSocketModeClient _socketClient;

    public SlackWorker(
        ISlackSocketModeClient socketClient,
        IEnumerable<IMessageEventHandler> eventHandlers,
        [FromKeyedServices("help")] IMessageEventHandler helpMessageHandler,
        IEnumerable<IReactionHandler> reactionHandlers,
        IMemberRepository memberRepository,
        ILogger<SlackWorker> logger)
    {
        _socketClient = socketClient;
        _helpMessageHandler = helpMessageHandler;
        _reactionHandlers = reactionHandlers;
        _memberRepository = memberRepository;
        _logger = logger;
        IList<IMessageEventHandler> eventHandlerList = eventHandlers.ToList();
        _conversationHandlers = eventHandlerList.OfType<ConversationMessageHandler>().ToList();
        _nonConversationHandlers = eventHandlerList.Except(_conversationHandlers).ToList();
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


            foreach (var handler in _nonConversationHandlers.Where(handler => handler.CanHandle(message)))
            {
                await handler.HandleAsync(message);
                return;
            }

            var conversationHandler = _conversationHandlers.Single();
            if (conversationHandler.CanHandle(message))
            {
                await conversationHandler.HandleAsync(message);
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