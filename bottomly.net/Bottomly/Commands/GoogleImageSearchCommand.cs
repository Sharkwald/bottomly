using Bottomly.Configuration;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands;

public class GoogleImageSearchCommand : ICommand
{
    private readonly string _cseId;
    private readonly CustomSearchAPIService _service;

    public GoogleImageSearchCommand(IOptions<BottomlyOptions> options)
    {
        _cseId = options.Value.GoogleCseId;
        _service = new CustomSearchAPIService(
            new BaseClientService.Initializer { ApiKey = options.Value.GoogleApiKey });
    }

    public string GetPurpose() => "Performs a google image search and returns the top hit.";

    public virtual async Task<GoogleSearchResult?> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return null;
        }

        var request = _service.Cse.List();
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