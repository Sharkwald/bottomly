using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Bottomly.Commands;

public abstract record UrbanResult;
public record UrbanSuccessResult(string Definition) : UrbanResult;
public record UrbanNotFoundResult : UrbanResult;
public record UrbanEmptyInputResult : UrbanResult;
public record UrbanErrorResult(string Error) : UrbanResult;

public class UrbanSearchCommand(IHttpClientFactory httpClientFactory, ILogger<UrbanSearchCommand> logger) : ICommand
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public string GetPurpose() => "Tells you what something _really_ means.";

    public virtual async Task<UrbanResult> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new UrbanEmptyInputResult();
        }

        try
        {
            var url = $"http://api.urbandictionary.com/v0/define?term={Uri.EscapeDataString(searchTerm)}";
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var list = doc.RootElement.GetProperty("list");

            if (list.GetArrayLength() == 0)
            {
                return new UrbanNotFoundResult();
            }

            var index = Random.Shared.Next(list.GetArrayLength());
            var definition = list[index].GetProperty("definition").GetString();
            return definition is not null
                ? new UrbanSuccessResult(definition)
                : new UrbanNotFoundResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing Urban Dictionary search");
            return new UrbanErrorResult(ex.Message);
        }
    }
}