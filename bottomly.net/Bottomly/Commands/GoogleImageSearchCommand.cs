using Bottomly.Configuration;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands;

public sealed class GoogleImageSearchCommand(IOptions<BottomlyOptions> options) : ICommand
{
    private readonly string _apiKey = options.Value.GoogleApiKey;
    private readonly string _cseId = options.Value.GoogleCseId;

    public string GetPurpose() => "Performs a google image search and returns the top hit.";

    public async Task<GoogleSearchResult?> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return null;
        }

        var service = new CustomSearchAPIService(new BaseClientService.Initializer { ApiKey = _apiKey });
        var request = service.Cse.List();
        request.Q = searchTerm;
        request.Cx = _cseId;
        request.Num = 1;
        request.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;

        var result = await request.ExecuteAsync();
        if (result.SearchInformation?.TotalResults == "0" || result.Items is null || !result.Items.Any())
        {
            return null;
        }

        var top = result.Items[0];
        return new GoogleSearchResult(top.Title, top.Link);
    }
}