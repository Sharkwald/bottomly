using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Bottomly.Commands;

public abstract record WikipediaResult;
public record WikipediaSuccessResult(string Text, string Link) : WikipediaResult;
public record WikipediaNotFoundResult : WikipediaResult;
public record WikipediaEmptyInputResult : WikipediaResult;
public record WikipediaErrorResult(string Error) : WikipediaResult;

public class WikipediaSearchCommand(IHttpClientFactory httpClientFactory, ILogger<WikipediaSearchCommand> logger) : ICommand
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public string GetPurpose() => "Performs a wikipedia search and returns the top hit.";

    public virtual async Task<WikipediaResult> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new WikipediaEmptyInputResult();
        }

        try
        {
            var url =
                $"https://en.wikipedia.org/w/api.php?action=opensearch&format=json&search={Uri.EscapeDataString(searchTerm)}";
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Bottomly/1.0");
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            // Response is: [searchTerm, [titles], [descriptions], [links]]
            var titles = root[1];
            var links = root[3];

            return titles.GetArrayLength() == 0
                ? new WikipediaNotFoundResult()
                : new WikipediaSuccessResult(titles[0].GetString()!, links[0].GetString()!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing Wikipedia search");
            return new WikipediaErrorResult(ex.Message);
        }
    }
}