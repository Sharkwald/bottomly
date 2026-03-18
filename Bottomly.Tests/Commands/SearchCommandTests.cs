using Bottomly.Commands.Search;
using Bottomly.Configuration;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Net;

namespace Bottomly.Tests.Commands;

public class SearchCommandTests
{
    private static readonly IOptions<BottomlyOptions> Options =
        Microsoft.Extensions.Options.Options.Create(new BottomlyOptions
        {
            BraveApiKey = "fake-key"
        });

    private static SearchCommand CreateCommand(string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(Options, NullLogger<SearchCommand>.Instance,
            TestHelpers.CreateHttpClientFactory(responseJson, statusCode));

    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsEmptySearchTermErrorResult()
    {
        var command = new SearchCommand(Options, NullLogger<SearchCommand>.Instance,
            TestHelpers.CreateHttpClientFactory(string.Empty));

        var result = await command.ExecuteAsync("");

        result.ShouldBeOfType<EmptySearchTermErrorResult>();
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceInput_ReturnsEmptySearchTermErrorResult()
    {
        var command = new SearchCommand(Options, NullLogger<SearchCommand>.Instance,
            TestHelpers.CreateHttpClientFactory(string.Empty));

        var result = await command.ExecuteAsync("   ");

        result.ShouldBeOfType<EmptySearchTermErrorResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsResults_ReturnsSearchResultWithTitleAndLink()
    {
        const string json = """
            {
              "type": "search",
              "web": {
                "results": [{ "title": "DotNet", "url": "https://dotnet.microsoft.com" }]
              }
            }
            """;

        var result = await CreateCommand(json).ExecuteAsync("dotnet");

        var searchResult = result.ShouldBeOfType<SearchResult>();
        searchResult.Title.ShouldBe("DotNet");
        searchResult.Link.ShouldBe("https://dotnet.microsoft.com");
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsEmptyResults_ReturnsNoResultsFoundResult()
    {
        const string json = """{ "type": "search", "web": { "results": [] } }""";

        var result = await CreateCommand(json).ExecuteAsync("anything");

        result.ShouldBeOfType<NoResultsFoundResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsNoWebProperty_ReturnsNoResultsFoundResult()
    {
        const string json = """{ "type": "search" }""";

        var result = await CreateCommand(json).ExecuteAsync("anything");

        result.ShouldBeOfType<NoResultsFoundResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsError_ReturnsSearchApiErrorResult()
    {
        const string errorJson = """{ "message": "Invalid subscription token" }""";

        var result = await CreateCommand(errorJson, HttpStatusCode.Unauthorized).ExecuteAsync("anything");

        var errorResult = result.ShouldBeOfType<SearchApiErrorResult>();
        errorResult.Error.ShouldBe("Invalid subscription token");
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsServerError_ReturnsSearchApiErrorResult()
    {
        var result = await CreateCommand("{}", HttpStatusCode.InternalServerError).ExecuteAsync("anything");

        result.ShouldBeOfType<SearchApiErrorResult>();
    }
}