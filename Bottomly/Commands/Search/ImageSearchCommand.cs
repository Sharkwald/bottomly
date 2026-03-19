using System.Text.Json;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands.Search;

public class ImageSearchCommand(
    IOptions<BottomlyOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<ImageSearchCommand> logger) : SearchCommandBase(options, httpClientFactory, logger)
{
    private const string BaseUrl = "https://api.search.brave.com/res/v1/images/search";

    public override string GetPurpose() => "Performs an image search and returns the top hit.";

    protected override string BuildUrl(string searchTerm) =>
        $"{BaseUrl}?q={Uri.EscapeDataString(searchTerm)}&count=1";

    protected override SearchCommandResult ExtractFirstResult(JsonElement root)
    {
        if (!root.TryGetProperty("results", out var results))
        {
            return new NoResultsFoundResult();
        }

        if (results.GetArrayLength() == 0)
        {
            return new NoResultsFoundResult();
        }

        var first = results[0];
        var title = first.TryGetProperty("title", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        if (!first.TryGetProperty("properties", out var props))
        {
            return new NoResultsFoundResult();
        }

        var url = props.TryGetProperty("url", out var u) ? u.GetString() ?? string.Empty : string.Empty;
        return new SearchResult(title, url);
    }
}