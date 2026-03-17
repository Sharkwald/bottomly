using System.Text.Json;

namespace Bottomly.Commands;

public class UrbanSearchCommand(IHttpClientFactory httpClientFactory) : ICommand
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public string GetPurpose() => "Tells you what something _really_ means.";

    public virtual async Task<string?> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return null;
        }

        var url = $"http://api.urbandictionary.com/v0/define?term={Uri.EscapeDataString(searchTerm)}";
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var list = doc.RootElement.GetProperty("list");

        if (list.GetArrayLength() == 0)
        {
            return null;
        }

        var index = Random.Shared.Next(list.GetArrayLength());
        return list[index].GetProperty("definition").GetString();
    }
}