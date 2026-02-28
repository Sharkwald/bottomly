using System.Text.Json;

namespace Bottomly.Commands;

public record WikipediaResult(string Text, string Link);

public class WikipediaSearchCommand(IHttpClientFactory httpClientFactory) : ICommand
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public string GetPurpose() => "Performs a wikipedia search and returns the top hit.";

    public virtual async Task<WikipediaResult?> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return null;
        }

        var url =
            $"https://en.wikipedia.org/w/api.php?action=opensearch&format=json&search={Uri.EscapeDataString(searchTerm)}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        // Response is: [searchTerm, [titles], [descriptions], [links]]
        var titles = root[1];
        var links = root[3];

        if (titles.GetArrayLength() == 0)
        {
            return null;
        }

        return new WikipediaResult(titles[0].GetString()!, links[0].GetString()!);
    }
}