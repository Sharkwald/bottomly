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
                x.SelectedModel = "gpt-oss:120bcloud";
            })
            .AddChatClient();

        var ollamaApiKey = builder.Configuration["bottomly_ollama_api_key"] ?? string.Empty;

        // The built-in resilience settings are super aggressive, with a 10s timeout.
        // The cloud Ollama endpoint cold-starts: the first request typically fails, but retries succeed in <30s.
        // AttemptTimeout is kept low (60s) so we abandon a stalled attempt quickly and let the retry fire fast,
        // rather than waiting the full 4 minutes we previously allowed.
#pragma warning disable EXTEXP0001
        builder.Services.AddHttpClient("bottomlymodel_httpClient")
            .ConfigureHttpClient(c => c.DefaultRequestHeaders.Add("Authorization", $"Bearer {ollamaApiKey}"))
            .RemoveAllResilienceHandlers()
#pragma warning restore EXTEXP0001
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(4);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(60);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(3);
                options.Retry.UseJitter = false;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
            });

        builder.Services.AddTransient<ILlmClient, LlmClient>();

        return builder;
    }
}