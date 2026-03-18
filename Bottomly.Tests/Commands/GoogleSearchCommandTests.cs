using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Net;

namespace Bottomly.Tests.Commands;

public class GoogleSearchCommandTests
{
    private static readonly IOptions<BottomlyOptions> Options =
        Microsoft.Extensions.Options.Options.Create(new BottomlyOptions
        {
            GoogleApiKey = "fake-key",
            GoogleCseId = "fake-cse"
        });

    private static GoogleSearchCommand CreateCommand(string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(Options, NullLogger<GoogleSearchCommand>.Instance,
            TestHelpers.CreateHttpClientFactory(responseJson, statusCode));

    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsEmptySearchTermErrorResult()
    {
        var command = new GoogleSearchCommand(Options, NullLogger<GoogleSearchCommand>.Instance,
            TestHelpers.CreateHttpClientFactory(string.Empty));

        var result = await command.ExecuteAsync("");

        result.ShouldBeOfType<EmptySearchTermErrorResult>();
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceInput_ReturnsEmptySearchTermErrorResult()
    {
        var command = new GoogleSearchCommand(Options, NullLogger<GoogleSearchCommand>.Instance,
            TestHelpers.CreateHttpClientFactory(string.Empty));

        var result = await command.ExecuteAsync("   ");

        result.ShouldBeOfType<EmptySearchTermErrorResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsResults_ReturnsGoogleSearchResultWithTitleAndLink()
    {
        const string json = """
            {
              "searchInformation": { "totalResults": "1" },
              "items": [{ "title": "DotNet", "link": "https://dotnet.microsoft.com" }]
            }
            """;

        var result = await CreateCommand(json).ExecuteAsync("dotnet");

        var searchResult = result.ShouldBeOfType<GoogleSearchResult>();
        searchResult.Title.ShouldBe("DotNet");
        searchResult.Link.ShouldBe("https://dotnet.microsoft.com");
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsZeroTotalResults_ReturnsNoResultsFoundResult()
    {
        const string json = """{ "searchInformation": { "totalResults": "0" } }""";

        var result = await CreateCommand(json).ExecuteAsync("anything");

        result.ShouldBeOfType<NoResultsFoundResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsNullItems_ReturnsNoResultsFoundResult()
    {
        const string json = """{ "searchInformation": { "totalResults": "1" } }""";

        var result = await CreateCommand(json).ExecuteAsync("anything");

        result.ShouldBeOfType<NoResultsFoundResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsError_ReturnsGoogleApiErrorResult()
    {
        const string errorJson = """
            {
              "error": {
                "code": 403,
                "message": "API key expired",
                "errors": [{ "domain": "global", "reason": "forbidden", "message": "API key expired" }]
              }
            }
            """;

        var result = await CreateCommand(errorJson, HttpStatusCode.Forbidden).ExecuteAsync("anything");

        var errorResult = result.ShouldBeOfType<GoogleApiErrorResult>();
        errorResult.Error.ShouldBe("API key expired");
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsServerError_ReturnsGoogleApiErrorResult()
    {
        var result = await CreateCommand("{}", HttpStatusCode.InternalServerError).ExecuteAsync("anything");

        result.ShouldBeOfType<GoogleApiErrorResult>();
    }
}