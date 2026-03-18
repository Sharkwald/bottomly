using System.Text.Json;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands;

public abstract record GoogleCommandResult;

public record GoogleSearchResult(string Title, string Link) : GoogleCommandResult;

public record GoogleApiErrorResult(string Error) : GoogleCommandResult;

public record NoResultsFoundResult : GoogleCommandResult;

public record EmptySearchTermErrorResult : GoogleCommandResult;

public class GoogleSearchCommand(
    IOptions<BottomlyOptions> options,
    ILogger<GoogleSearchCommand> logger,
    IHttpClientFactory httpClientFactory) : ICommand
{
    private const string BaseUrl = "https://customsearch.googleapis.com/customsearch/v1";
    private readonly string _apiKey = options.Value.GoogleApiKey;
    private readonly string _cseId = options.Value.GoogleCseId;

    public string GetPurpose() => "Performs a google search and returns the top hit.";

    public virtual async Task<GoogleCommandResult> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return new EmptySearchTermErrorResult();

        try
        {
            var client = httpClientFactory.CreateClient();
            var url = $"{BaseUrl}?key={_apiKey}&cx={_cseId}&q={Uri.EscapeDataString(searchTerm)}&num=1";
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = ExtractErrorMessage(body) ?? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                logger.LogError("Google search API error {StatusCode}: {Message}", response.StatusCode, errorMessage);
                return new GoogleApiErrorResult(errorMessage);
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("searchInformation", out var info) &&
                info.TryGetProperty("totalResults", out var total) &&
                total.GetString() == "0")
                return new NoResultsFoundResult();

            if (!root.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
                return new NoResultsFoundResult();

            var first = items[0];
            var title = first.GetProperty("title").GetString() ?? string.Empty;
            var link = first.GetProperty("link").GetString() ?? string.Empty;
            return new GoogleSearchResult(title, link);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error executing Google search");
            return new GoogleApiErrorResult(e.Message);
        }
    }

    private static string? ExtractErrorMessage(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var msg))
                return msg.GetString();
        }
        catch (JsonException) { }
        return null;
    }
}
