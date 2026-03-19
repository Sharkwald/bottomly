using Bottomly.Commands.Search;
using Bottomly.Configuration;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit.Abstractions;

namespace Bottomly.Tests.Commands.Integration;

/// <summary>
///     Integration tests that call the real Brave Search API with image search.
///     Credentials are resolved from the standard .NET configuration stack:
///     1. User secrets stored against the main Bottomly app project (local dev —
///     run `dotnet user-secrets set "bottomly_brave_api_key" "..." --project Bottomly`)
///     2. Environment variable BOTTOMLY_BRAVE_API_KEY
///     (CI — injected from GitHub repository secrets via the workflow env block)
///     Tests no-op silently when credentials are absent, so the suite stays green
///     for contributors without keys. When credentials are present but expired or
///     invalid the tests will fail, which is exactly the failure mode they exist to expose.
/// </summary>
public class ImageSearchCommandIntegrationTests
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddUserSecrets<SearchCommand>()
        .AddEnvironmentVariables()
        .Build();

    private readonly ILogger<ImageSearchCommand> _logger;

    public ImageSearchCommandIntegrationTests(ITestOutputHelper outputHelper)
    {
        _logger = XUnitLogger.CreateLogger<ImageSearchCommand>(outputHelper);
    }

    private static string? ApiKey => Configuration["bottomly_brave_api_key"];

    private static bool CredentialsAvailable => !string.IsNullOrWhiteSpace(ApiKey);

    private ImageSearchCommand CreateCommand()
    {
        var factory = new DefaultHttpClientFactory();
        return new ImageSearchCommand(Options.Create(new BottomlyOptions
        {
            BraveApiKey = ApiKey!
        }), factory, _logger);
    }

    private sealed class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsEmptySearchTermErrorResult()
    {
        if (!CredentialsAvailable)
        {
            return;
        }

        var result = await CreateCommand().ExecuteAsync("");

        result.ShouldBeOfType<EmptySearchTermErrorResult>();
    }

    [Fact]
    public async Task ExecuteAsync_KnownSearchTerm_ReturnsResultWithLink()
    {
        if (!CredentialsAvailable)
        {
            return;
        }

        var result = await CreateCommand().ExecuteAsync("GitHub");

        result.ShouldBeOfType<SearchResult>();
        var searchResult = (SearchResult)result;
        searchResult.Title.ShouldNotBeNullOrEmpty();
        searchResult.Link.ShouldNotBeNullOrEmpty();
        searchResult.Link.ShouldStartWith("http");
    }

    [Fact]
    public async Task ExecuteAsync_KnownSearchTerm_ReturnsRelevantResult()
    {
        if (!CredentialsAvailable)
        {
            return;
        }

        var result = await CreateCommand().ExecuteAsync("Wikipedia logo");

        result.ShouldBeOfType<SearchResult>();
        var searchResult = (SearchResult)result;
        searchResult.Link.ShouldNotBeNullOrEmpty();
        searchResult.Link.ShouldStartWith("http");
    }
}