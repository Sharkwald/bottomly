using Bottomly.Commands.Google;
using Bottomly.Configuration;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit.Abstractions;

namespace Bottomly.Tests.Commands.Google;

/// <summary>
///     Integration tests that call the real Google Custom Search API with image search.
///     Credentials are resolved from the standard .NET configuration stack:
///     1. User secrets stored against the main Bottomly app project (local dev —
///     run `dotnet user-secrets set "bottomly_google_api_key" "..." --project Bottomly`)
///     2. Environment variables BOTTOMLY_GOOGLE_API_KEY / BOTTOMLY_GOOGLE_CSE_ID
///     (CI — injected from GitHub repository secrets via the workflow env block)
///     Tests no-op silently when credentials are absent, so the suite stays green
///     for contributors without keys. When credentials are present but expired or
///     invalid the tests will fail, which is exactly the failure mode they exist to expose.
/// </summary>
public class GoogleImageSearchCommandIntegrationTests
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddUserSecrets<GoogleSearchCommand>()
        .AddEnvironmentVariables()
        .Build();

    private readonly ILogger<GoogleImageSearchCommand> _logger;

    public GoogleImageSearchCommandIntegrationTests(ITestOutputHelper outputHelper)
    {
        _logger = XUnitLogger.CreateLogger<GoogleImageSearchCommand>(outputHelper);
    }

    private static string? ApiKey => Configuration["bottomly_google_api_key"];
    private static string? CseId => Configuration["bottomly_google_cse_id"];

    private static bool CredentialsAvailable =>
        !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(CseId);

    private GoogleImageSearchCommand CreateCommand()
    {
        var factory = new DefaultHttpClientFactory();
        return new GoogleImageSearchCommand(Options.Create(new BottomlyOptions
        {
            GoogleApiKey = ApiKey!,
            GoogleCseId = CseId!
        }), factory, _logger);
    }

    private sealed class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsEmptySearchTermErrorResult()
    {
        if (!CredentialsAvailable) return;

        var result = await CreateCommand().ExecuteAsync("");

        result.ShouldBeOfType<EmptySearchTermErrorResult>();
    }

    [Fact]
    public async Task ExecuteAsync_KnownSearchTerm_ReturnsResultWithLink()
    {
        if (!CredentialsAvailable) return;

        var result = await CreateCommand().ExecuteAsync("GitHub");

        result.ShouldBeOfType<GoogleSearchResult>();
        var searchResult = (GoogleSearchResult)result;
        searchResult.Title.ShouldNotBeNullOrEmpty();
        searchResult.Link.ShouldNotBeNullOrEmpty();
        searchResult.Link.ShouldStartWith("http");
    }

    [Fact]
    public async Task ExecuteAsync_KnownSearchTerm_ReturnsRelevantResult()
    {
        if (!CredentialsAvailable) return;

        var result = await CreateCommand().ExecuteAsync("Wikipedia logo");

        result.ShouldBeOfType<GoogleSearchResult>();
        var searchResult = (GoogleSearchResult)result;
        searchResult.Link.ShouldNotBeNullOrEmpty();
        searchResult.Link.ShouldStartWith("http");
    }
}
