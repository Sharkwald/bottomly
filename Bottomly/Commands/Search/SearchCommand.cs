using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands.Search;

public class SearchCommand(
    IOptions<BottomlyOptions> options,
    ILogger<SearchCommand> logger,
    IHttpClientFactory httpClientFactory) : SearchCommandBase(options, httpClientFactory, logger)
{
    public override string GetPurpose() => "Performs a google search and returns the top hit.";
}
