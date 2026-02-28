using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.EventHandlers;
using Bottomly.Slack.EventHandlers.KarmaEventHandlers;
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
builder.Services.AddSingleton<AddKarmaCommand>();
builder.Services.AddSingleton<GetCurrentNetKarmaCommand>();
builder.Services.AddSingleton<GetCurrentKarmaReasonsCommand>();
builder.Services.AddSingleton<GetLeaderBoardCommand>();
builder.Services.AddSingleton<GetLoserBoardCommand>();
builder.Services.AddSingleton<GoogleSearchCommand>();
builder.Services.AddSingleton<GoogleImageSearchCommand>();
builder.Services.AddSingleton<GiphyCommand>();
builder.Services.AddSingleton<UrbanSearchCommand>();
builder.Services.AddSingleton<WikipediaSearchCommand>();
builder.Services.AddSingleton<RegSearchCommand>();
builder.Services.AddSingleton<ReleaseCommand>();

// Slack infrastructure
builder.Services.AddSingleton<SlackParser>();
builder.Services.AddSingleton<ISlackMessageBroker, SlackMessageBroker>();

// SlackNet
var slackBotToken = builder.Configuration["bottomly_slack_bot_token"] ?? string.Empty;
var slackAppToken = builder.Configuration["bottomly_slack_app_token"] ?? string.Empty;

builder.Services.AddSlackNet(c => c
    .UseApiToken(slackBotToken)
    .UseAppLevelToken(slackAppToken)
    .RegisterEventHandler<MessageEvent, SlackMessageEventDispatcher>()
    .RegisterEventHandler<ReactionAdded, SlackReactionEventDispatcher>());

// Event handlers (registered for IEventHandler collection, excluding Help which is separate)
builder.Services.AddSingleton<IEventHandler, GoogleEventHandler>();
builder.Services.AddSingleton<IEventHandler, GoogleImageEventHandler>();
builder.Services.AddSingleton<IEventHandler, UrbanEventHandler>();
builder.Services.AddSingleton<IEventHandler, WikipediaEventHandler>();
builder.Services.AddSingleton<IEventHandler, GiphyEventHandler>();
builder.Services.AddSingleton<IEventHandler, GetCurrentNetKarmaEventHandler>();
builder.Services.AddSingleton<IEventHandler, GetCurrentKarmaReasonsEventHandler>();
builder.Services.AddSingleton<IEventHandler, GetLeaderBoardEventHandler>();
builder.Services.AddSingleton<IEventHandler, GetLoserBoardEventHandler>();
builder.Services.AddSingleton<IEventHandler, RegEventHandler>();
builder.Services.AddSingleton<IEventHandler, ReleaseEventHandler>();
builder.Services.AddSingleton<IEventHandler, IncrementKarmaEventHandler>();
builder.Services.AddSingleton<IEventHandler, DecrementKarmaEventHandler>();

// Help handler (also registered as singleton for direct injection into SlackWorker)
builder.Services.AddSingleton<HelpEventHandler>();

// Reaction handlers
builder.Services.AddSingleton<IReactionHandler, AddKarmaReactionHandler>();

// Slack dispatchers and worker
builder.Services.AddSingleton<SlackWorker>();
builder.Services.AddSingleton<SlackMessageEventDispatcher>();
builder.Services.AddSingleton<SlackReactionEventDispatcher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<SlackWorker>());

builder.Build().Run();