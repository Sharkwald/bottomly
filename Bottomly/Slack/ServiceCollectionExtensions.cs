using Bottomly.Repositories;
using Bottomly.Slack.MembershipEventHandlers;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;
using Bottomly.Slack.ReactionHandlers;
using Bottomly.Slack.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlackNet.Events;
using SlackNet.Extensions.DependencyInjection;

namespace Bottomly.Slack;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Handlers registered with keyed DI (not via auto-discovery). Pass this to
    /// <c>RegisterEventHandlers</c> as the exclude list to prevent double-registration.
    /// </summary>
    public static readonly Type[] KeyedHandlerTypes =
    [
        typeof(HelpHandler),
        typeof(ConversationMessageHandler)
    ];

    public static IServiceCollection AddBottomlySlack(this IServiceCollection services, IConfiguration configuration)
    {
        var slackBotToken = configuration["bottomly_slack_bot_token"] ?? string.Empty;
        var slackAppToken = configuration["bottomly_slack_app_token"] ?? string.Empty;

        // Slack infrastructure
        services.AddSingleton<SlackParser>();
        services.AddSingleton<ISlackMessageBroker, SlackMessageBroker>();

        // SlackNet
        services.AddSlackNet(c => c
            .UseApiToken(slackBotToken)
            .UseAppLevelToken(slackAppToken)
            .RegisterEventHandler<MemberJoinedChannel, SlackMemberAddedEventDispatcher>()
            .RegisterEventHandler<MessageEvent, SlackMessageEventDispatcher>()
            .RegisterEventHandler<ReactionAdded, SlackReactionEventDispatcher>());

        // Help handler — concrete type for its own resolution, decorated version keyed for SlackWorker injection
        services.AddSingleton<HelpHandler>();
        services.AddKeyedSingleton<IMessageEventHandler>(SlackWorker.HelpHandlerKey,
            (sp, _) => new TracingMessageEventHandlerDecorator(sp.GetRequiredService<HelpHandler>()));

        // Conversation handler — keyed registration so SlackWorker can inject it separately as a fallback
        services.AddSingleton<ConversationMessageHandler>();
        services.AddKeyedSingleton<IMessageEventHandler>(SlackWorker.ConversationHandlerKey,
            (sp, _) => new TracingMessageEventHandlerDecorator(sp.GetRequiredService<ConversationMessageHandler>()));

        // Reaction handlers (wrapped in tracing decorator)
        services.AddSingleton<KarmaReactionMap>();
        services.AddSingleton<IReactionHandler>(sp =>
            new TracingReactionHandlerDecorator(
                ActivatorUtilities.CreateInstance<AddKarmaReactionHandler>(sp)));

        // Membership handlers
        services.AddSingleton<MemberJoinedEventHandler>();

        // Slack dispatchers and worker
        services.AddSingleton<SlackWorker>();
        services.AddSingleton<SlackMessageEventDispatcher>();
        services.AddSingleton<SlackReactionEventDispatcher>();
        services.AddSingleton<SlackMemberAddedEventDispatcher>();
        services.AddHostedService<MemberCachePopulator>();
        services.AddHostedService(sp => sp.GetRequiredService<SlackWorker>());

        return services;
    }
}