using System.Text.Json;
using Bottomly.Configuration;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands;

public class GoogleImageSearchCommand(IOptions<BottomlyOptions> options, IHttpClientFactory httpClientFactory) : ICommand
{
    private const string BaseUrl = "https://customsearch.googleapis.com/customsearch/v1";
    private readonly string _apiKey = options.Value.GoogleApiKey;
    private readonly string _cseId = options.Value.GoogleCseId;

    public string GetPurpose() => "Performs a google image search and returns the top hit.";

    public virtual async Task<GoogleCommandResult> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return new EmptySearchTermErrorResult();

        try
        {
            var client = httpClientFactory.CreateClient();
            var url = $"{BaseUrl}?key={_apiKey}&cx={_cseId}&q={Uri.EscapeDataString(searchTerm)}&num=1&searchType=image";
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = ExtractErrorMessage(body) ?? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
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
