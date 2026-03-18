using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands.Search;

public class ImageSearchCommand(
    IOptions<BottomlyOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<ImageSearchCommand> logger) : SearchCommandBase(options, httpClientFactory, logger)
{
    protected override string ExtraQueryParams => "&searchType=image";

    public override string GetPurpose()
    {
        return "Performs a google image search and returns the top hit.";
    }
}
