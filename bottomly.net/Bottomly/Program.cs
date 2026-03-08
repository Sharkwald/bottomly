using System.Reflection;
using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.LlmBot;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MembershipEventHandlers;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Slack.ReactionHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SlackNet.Events;
using SlackNet.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddUserSecrets(typeof(Program).Assembly).AddJsonFile("appsettings.json");

// MongoDB
builder.AddMongoDBClient("mongodb");
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("bottomly");
});

// Configuration
builder.Services.Configure<BottomlyOptions>(opts =>
{
    opts.SlackBotToken = builder.Configuration["bottomly_slack_bot_token"] ?? string.Empty;
    opts.SlackAppToken = builder.Configuration["bottomly_slack_app_token"] ?? string.Empty;
    opts.GoogleApiKey = builder.Configuration["bottomly_google_api_key"] ?? string.Empty;
    opts.GoogleCseId = builder.Configuration["bottomly_google_cse_id"] ?? string.Empty;
    opts.Prefix = builder.Configuration["bottomly_prefix"] ?? "!";
    opts.GiphyApiKey = builder.Configuration["bottomly_giphy_api_key"] ?? string.Empty;
    opts.Environment = builder.Configuration["bottomly_env"] ?? "live";
    opts.GitHubToken = builder.Configuration["bottomly_github_token"] ?? string.Empty;
});

var opts = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<BottomlyOptions>>();

// HTTP
builder.Services.AddHttpClient();

// Repositories
builder.Services.AddSingleton<IKarmaRepository, KarmaRepository>();
builder.Services.AddSingleton<IMemberRepository, MemberRepository>();

// Commands
builder.RegisterCommands(Assembly.GetExecutingAssembly());

// Slack infrastructure
builder.Services.AddSingleton<SlackParser>();
builder.Services.AddSingleton<ISlackMessageBroker, SlackMessageBroker>();

// SlackNet
var slackBotToken = builder.Configuration["bottomly_slack_bot_token"] ?? string.Empty;
var slackAppToken = builder.Configuration["bottomly_slack_app_token"] ?? string.Empty;

builder.Services.AddSlackNet(c => c
    .UseApiToken(slackBotToken)
    .UseAppLevelToken(slackAppToken)
    .RegisterEventHandler<MemberJoinedChannel, SlackMemberAddedEventDispatcher>()
    .RegisterEventHandler<MessageEvent, SlackMessageEventDispatcher>()
    .RegisterEventHandler<ReactionAdded, SlackReactionEventDispatcher>());

// Event handlers (registered for IEventHandler collection, excluding Help which is separate)
builder.RegisterEventHandlers(Assembly.GetExecutingAssembly());

// Help handler (also registered as singleton for direct injection into SlackWorker)
builder.Services.AddSingleton<HelpHandler>();

// Reaction handlers
builder.Services.AddSingleton<KarmaReactionMap>();
builder.Services.AddSingleton<IReactionHandler, AddKarmaReactionHandler>();

// Membership handlers
builder.Services.AddSingleton<MemberJoinedEventHandler>();

// Slack dispatchers and worker
builder.Services.AddSingleton<SlackWorker>();
builder.Services.AddSingleton<SlackMessageEventDispatcher>();
builder.Services.AddSingleton<SlackReactionEventDispatcher>();
builder.Services.AddSingleton<SlackMemberAddedEventDispatcher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<SlackWorker>());

// LLM Support
builder.AddOllamaApiClient("qwen3")
    .AddChatClient();

// The built-in resilience settings are super aggressive, with a 10s timeout.
// Running locally Qwen3 takes ~2m to respond to simple queries, so we need to override the defaults.
#pragma warning disable EXTEXP0001
builder.Services.AddHttpClient("qwen3_httpClient")
    .RemoveAllResilienceHandlers()
#pragma warning restore EXTEXP0001
    .AddStandardResilienceHandler(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(4);
    });

builder.Services.AddTransient<LlmMessageBroker>();

// Seeding
builder.Services.AddSingleton<MemberlistPopulator>();

var app = builder.Build();

var populator = app.Services.GetRequiredService<MemberlistPopulator>();
await populator.PopulateMembers();

app.Run();


public static class HostBuilderExtensions
{
    extension(HostApplicationBuilder builder)
    {
        public void RegisterEventHandlers(Assembly assembly) =>
            assembly.GetTypes()
                .Where(t => typeof(IMessageEventHandler).IsAssignableFrom(t) &&
                            t is { IsInterface: false, IsAbstract: false })
                .Where(t => t.Name != nameof(HelpHandler))
                .ToList()
                .ForEach(t => builder.Services.AddSingleton(typeof(IMessageEventHandler), t));

        public void RegisterCommands(Assembly assembly) =>
            assembly.GetTypes()
                .Where(t => typeof(ICommand).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .ToList()
                .ForEach(t => builder.Services.AddSingleton(t));
    }
}