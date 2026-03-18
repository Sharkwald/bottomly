using System.Text.Json;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands.Search;

public class SearchCommand(
    IOptions<BottomlyOptions> options,
    ILogger<SearchCommand> logger,
    IHttpClientFactory httpClientFactory) : SearchCommandBase(options, httpClientFactory, logger)
{
    private const string BaseUrl = "https://api.search.brave.com/res/v1/web/search";

    public override string GetPurpose() => "Performs a web search and returns the top hit.";

    protected override string BuildUrl(string searchTerm) =>
        $"{BaseUrl}?q={Uri.EscapeDataString(searchTerm)}&count=1";

    protected override SearchCommandResult ExtractFirstResult(JsonElement root)
    {
        if (!root.TryGetProperty("web", out var web)) return new NoResultsFoundResult();
        if (!web.TryGetProperty("results", out var results)) return new NoResultsFoundResult();
        if (results.GetArrayLength() == 0) return new NoResultsFoundResult();

        var first = results[0];
        var title = first.TryGetProperty("title", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        var url = first.TryGetProperty("url", out var u) ? u.GetString() ?? string.Empty : string.Empty;
        return new SearchResult(title, url);
    }
}
