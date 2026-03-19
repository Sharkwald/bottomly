using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bottomly.LlmBot;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddBottomlyLlm(this IHostApplicationBuilder builder)
    {
        builder.AddOllamaApiClient("bottomlymodel", x =>
            {
                x.Endpoint = new Uri("https://ollama.com");
                x.SelectedModel = "qwen3.5:cloud";
            })
            .AddChatClient();

        var ollamaApiKey = builder.Configuration["bottomly_ollama_api_key"] ?? string.Empty;

        // The built-in resilience settings are super aggressive, with a 10s timeout.
        // Running locally Qwen3 takes ~2m to respond to simple queries, so we need to override the defaults.
#pragma warning disable EXTEXP0001
        builder.Services.AddHttpClient("bottomlymodel_httpClient")
            .ConfigureHttpClient(c => c.DefaultRequestHeaders.Add("Authorization", $"Bearer {ollamaApiKey}"))
            .RemoveAllResilienceHandlers()
#pragma warning restore EXTEXP0001
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(4);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(8);
            });

        builder.Services.AddTransient<ILlmClient, LlmClient>();

        return builder;
    }
}
