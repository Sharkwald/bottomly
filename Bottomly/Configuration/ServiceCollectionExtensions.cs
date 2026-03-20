using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bottomly.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBottomlyConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BottomlyOptions>(opts =>
        {
            opts.SlackBotToken = configuration["bottomly_slack_bot_token"] ?? string.Empty;
            opts.SlackAppToken = configuration["bottomly_slack_app_token"] ?? string.Empty;
            opts.GoogleApiKey = configuration["bottomly_google_api_key"] ?? string.Empty;
            opts.GoogleCseId = configuration["bottomly_google_cse_id"] ?? string.Empty;
            opts.Prefix = configuration["bottomly_prefix"] ?? "!";
            opts.GiphyApiKey = configuration["bottomly_giphy_api_key"] ?? string.Empty;
            opts.Environment = configuration["bottomly_env"] ?? "live";
            opts.GitHubToken = configuration["bottomly_github_token"] ?? string.Empty;
            opts.BraveApiKey = configuration["bottomly_brave_api_key"] ?? string.Empty;
            opts.OllamaApiKey = configuration["bottomly_ollama_api_key"] ?? string.Empty;
        });

        return services;
    }
}