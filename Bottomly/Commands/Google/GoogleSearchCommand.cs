using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Commands.Google;

public class GoogleSearchCommand(
    IOptions<BottomlyOptions> options,
    ILogger<GoogleSearchCommand> logger,
    IHttpClientFactory httpClientFactory) : GoogleCommandBase(options, httpClientFactory, logger)
{
    protected override string? ExtraQueryParams => null;

    public override string GetPurpose() => "Performs a google search and returns the top hit.";
}
