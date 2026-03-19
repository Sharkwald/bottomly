using System.Text.Json;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands.Search;

public abstract class SearchCommandBase(
    IOptions<BottomlyOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger logger)
    : ICommand
{
    private readonly string _apiKey = options.Value.BraveApiKey;

    public abstract string GetPurpose();

    protected abstract string BuildUrl(string searchTerm);
    protected abstract SearchCommandResult ExtractFirstResult(JsonElement root);

    public virtual async Task<SearchCommandResult> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new EmptySearchTermErrorResult();
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(searchTerm));
            request.Headers.Add("X-Subscription-Token", _apiKey);
            request.Headers.Add("Accept", "application/json");

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await ExtractErrorMessageAsync(response);
                logger?.LogError("Brave search API error {StatusCode}: {Message}", response.StatusCode, errorMessage);
                return new SearchApiErrorResult(errorMessage);
            }

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            return ExtractFirstResult(doc.RootElement);
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Error executing search");
            return new SearchApiErrorResult(e.Message);
        }
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var msg))
            {
                return msg.GetString()!;
            }
        }
        catch (JsonException)
        {
            // swallow.
        }

        return response.ReasonPhrase ?? "Unknown error";
    }
}