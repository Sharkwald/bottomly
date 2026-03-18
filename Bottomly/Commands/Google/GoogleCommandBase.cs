using System.Text.Json;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands.Google;

public abstract class GoogleCommandBase(
    IOptions<BottomlyOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger logger)
    : ICommand
{
    private const string BaseUrl = "https://customsearch.googleapis.com/customsearch/v1";
    private readonly string _apiKey = options.Value.GoogleApiKey;
    private readonly string _cseId = options.Value.GoogleCseId;

    protected virtual string ExtraQueryParams => string.Empty;

    public abstract string GetPurpose();

    public virtual async Task<GoogleCommandResult> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return new EmptySearchTermErrorResult();

        try
        {
            var client = httpClientFactory.CreateClient();
            var url =
                $"{BaseUrl}?key={_apiKey}" +
                $"&cx={_cseId}" +
                $"&q={Uri.EscapeDataString(searchTerm)}&num=1{ExtraQueryParams}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = ExtractErrorMessage(response);
                logger?.LogError("Google search API error {StatusCode}: {Message}", response.StatusCode, errorMessage);
                return new GoogleApiErrorResult(errorMessage);
            }

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (!TryGetSearchResults(root, out var items)) return new NoResultsFoundResult();

            var first = items[0];
            var title = first.GetProperty("title").GetString() ?? string.Empty;
            var link = first.GetProperty("link").GetString() ?? string.Empty;
            return new GoogleSearchResult(title, link);
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Error executing Google search");
            return new GoogleApiErrorResult(e.Message);
        }
    }

    private static bool TryGetSearchResults(JsonElement root, out JsonElement results)
    {
        results = default;
        return !(root.TryGetProperty("searchInformation", out var info) &&
                 info.TryGetProperty("totalResults", out var total) &&
                 total.GetString() == "0") && root.TryGetProperty("items", out results) &&
               results.GetArrayLength() > 0;
    }

    private static string ExtractErrorMessage(HttpResponseMessage response)
    {
        try
        {
            using var doc = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
            if (doc.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var msg))
                return msg.GetString()!;
        }
        catch (JsonException)
        {
            // swallow.
        }

        return response.ReasonPhrase ?? "Unknown error";
    }
}