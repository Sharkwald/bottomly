using Bottomly.Configuration;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands.Google;

public class GoogleImageSearchCommand(
    IOptions<BottomlyOptions> options,
    IHttpClientFactory httpClientFactory) : GoogleCommandBase(options, httpClientFactory)
{
    protected override string? ExtraQueryParams => "&searchType=image";

    public override string GetPurpose() => "Performs a google image search and returns the top hit.";
}
