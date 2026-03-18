using Bottomly.Configuration;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands;

public abstract record GoogleCommandResult;

public record GoogleSearchResult(string Title, string Link) : GoogleCommandResult;

public record GoogleApiErrorResult(string Error) : GoogleCommandResult;

public record NoResultsFoundResult : GoogleCommandResult;

public record EmptySearchTermErrorResult : GoogleCommandResult;

public class GoogleSearchCommand : ICommand
{
    private readonly string _cseId;
    private readonly ILogger<GoogleSearchCommand> _logger;
    private readonly CustomSearchAPIService _service;

    public GoogleSearchCommand(IOptions<BottomlyOptions> options, ILogger<GoogleSearchCommand> logger)
    {
        _cseId = options.Value.GoogleCseId;
        _service = new CustomSearchAPIService(
            new BaseClientService.Initializer { ApiKey = options.Value.GoogleApiKey });
        _logger = logger;
    }

    internal GoogleSearchCommand(IOptions<BottomlyOptions> options, ILogger<GoogleSearchCommand> logger,
        Google.Apis.Http.IHttpClientFactory httpClientFactory)
    {
        _cseId = options.Value.GoogleCseId;
        _service = new CustomSearchAPIService(
            new BaseClientService.Initializer
            {
                ApiKey = options.Value.GoogleApiKey,
                HttpClientFactory = httpClientFactory
            });
        _logger = logger;
    }

    public string GetPurpose()
    {
        return "Performs a google search and returns the top hit.";
    }

    public virtual async Task<GoogleCommandResult> ExecuteAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return new EmptySearchTermErrorResult();

        try
        {
            var request = _service.Cse.List();
            request.Q = searchTerm;
            request.Cx = _cseId;
            request.Num = 1;

            var result = await request.ExecuteAsync();
            if (result.SearchInformation?.TotalResults == "0" || result.Items is null || !result.Items.Any())
                return new NoResultsFoundResult();

            var top = result.Items[0];
            return new GoogleSearchResult(top.Title, top.Link);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error executing Google search");
            return new GoogleApiErrorResult(e.Message);
        }
    }
}