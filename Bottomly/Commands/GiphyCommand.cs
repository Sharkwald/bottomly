using System.Text.Json;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands;

public class GiphyCommand(
    IHttpClientFactory httpClientFactory,
    IOptions<BottomlyOptions> options,
    ILogger<GiphyCommand> logger)
    : ICommand
{
    private readonly string _apiKey = options.Value.GiphyApiKey;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public string GetPurpose() => "Uses Giphy to find a gif matching the given search term";

    public virtual async Task<GiphyResult> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new GiphyBadInputResult();
        }

        try
        {
            var url =
                $"http://api.giphy.com/v1/gifs/translate?limit=1&api_key={_apiKey}&s={Uri.EscapeDataString(searchTerm)}";
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var data = doc.RootElement.GetProperty("data");

            if (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() == 0)
            {
                return new GiphyEmptyResult();
            }

            var gifUrl = data.GetProperty("url").GetString();
            return string.IsNullOrEmpty(gifUrl)
                ? new GiphyEmptyResult()
                : new GiphySuccessResult(gifUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing Giphy search");
            return new GiphyErrorResult(ex.Message);
        }
    }
}

public abstract record GiphyResult;

public record GiphyBadInputResult : GiphyResult;

public record GiphyErrorResult(string Error) : GiphyResult;

public record GiphyEmptyResult : GiphyResult;

public record GiphySuccessResult(string Url) : GiphyResult;