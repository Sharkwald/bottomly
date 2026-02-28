using System.Text.Json;
using Bottomly.Configuration;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands;

public class GiphyCommand(IHttpClientFactory httpClientFactory, IOptions<BottomlyOptions> options)
    : ICommand
{
    private readonly string _apiKey = options.Value.GiphyApiKey;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public string GetPurpose() => "Uses Giphy to find a gif matching the given search term";

    public virtual async Task<string?> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return null;
        }

        var url =
            $"http://api.giphy.com/v1/gifs/translate?limit=1&api_key={_apiKey}&s={Uri.EscapeDataString(searchTerm)}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");

        if (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() == 0)
        {
            return null;
        }

        return data.GetProperty("url").GetString();
    }
}