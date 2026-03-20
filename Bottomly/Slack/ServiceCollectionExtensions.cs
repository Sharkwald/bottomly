using Bottomly.Repositories;
using Bottomly.Slack.MembershipEventHandlers;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Slack.ReactionHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlackNet.Events;
using SlackNet.Extensions.DependencyInjection;

namespace Bottomly.Slack;

public static class ServiceCollectionExtensions
{
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

        // Help handler (also registered as singleton for direct injection into SlackWorker)
        services.AddSingleton<HelpHandler>();

        // Reaction handlers
        services.AddSingleton<KarmaReactionMap>();
        services.AddSingleton<IReactionHandler, AddKarmaReactionHandler>();

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