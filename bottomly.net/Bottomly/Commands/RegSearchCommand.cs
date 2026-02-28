using HtmlAgilityPack;

namespace Bottomly.Commands;

public class RegSearchCommand(IHttpClientFactory httpClientFactory) : ICommand
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public string GetPurpose() => "AutoTrader reg lookup, because Jamie is lazy.";

    public async Task<string> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return "Registration missing";
        }

        searchTerm = searchTerm.Replace(" ", "").ToLower();

        if (searchTerm.Length > 7)
        {
            return "Registration too long.";
        }

        if (!searchTerm.All(char.IsLetterOrDigit))
        {
            return "Registration should not contain special characters";
        }

        searchTerm = searchTerm.Replace("i", "1");

        var url = $"https://www.vehiclecheck.co.uk/?vrm={searchTerm}";
        var response = await _httpClient.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        try
        {
            var colour = doc.GetElementbyId("VehicleColour")?.GetAttributeValue("value", "").Trim();
            var year = doc.GetElementbyId("RegistrationYear")?.GetAttributeValue("value", "").Trim();
            var make = doc.GetElementbyId("VehicleMake")?.GetAttributeValue("value", "").Trim();
            make = make is not null ? char.ToUpper(make[0]) + make[1..].ToLower() : make;
            var model = doc.GetElementbyId("VehicleModel")?.GetAttributeValue("value", "").Trim();
            var imgSrc = doc.GetElementbyId("searchResultCarImage")?.GetAttributeValue("src", "").Trim();
            var image = imgSrc is not null ? "https://www.vehiclecheck.co.uk" + imgSrc : "";

            return $"{colour} {make} {model} ({year}) {image}".Trim();
        }
        catch
        {
            var errorDiv = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'ErrorMessage')]");
            var errorText = errorDiv?.SelectSingleNode(".//h3")?.InnerText ?? "Unknown error";
            return errorText;
        }
    }
}