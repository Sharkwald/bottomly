using System.Text.Json;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands.Google;

public abstract class GoogleCommandBase : ICommand
{
    private const string BaseUrl = "https://customsearch.googleapis.com/customsearch/v1";
    private readonly string _apiKey;
    private readonly string _cseId;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger? _logger;

    protected GoogleCommandBase(IOptions<BottomlyOptions> options, IHttpClientFactory httpClientFactory,
        ILogger? logger = null)
    {
        _apiKey = options.Value.GoogleApiKey;
        _cseId = options.Value.GoogleCseId;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public abstract string GetPurpose();

    protected virtual string ExtraQueryParams => string.Empty;

    public virtual async Task<GoogleCommandResult> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return new EmptySearchTermErrorResult();

        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{BaseUrl}?key={_apiKey}&cx={_cseId}&q={Uri.EscapeDataString(searchTerm)}&num=1{ExtraQueryParams}";
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = ExtractErrorMessage(body) ?? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                _logger?.LogError("Google search API error {StatusCode}: {Message}", response.StatusCode, errorMessage);
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
            _logger?.LogError(e, "Error executing Google search");
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
