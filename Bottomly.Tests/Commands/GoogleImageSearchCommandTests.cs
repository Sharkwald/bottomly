using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using System.Net;

namespace Bottomly.Tests.Commands;

public class GoogleImageSearchCommandTests
{
    private static readonly IOptions<BottomlyOptions> Options =
        Microsoft.Extensions.Options.Options.Create(new BottomlyOptions { GoogleApiKey = "key", GoogleCseId = "cse" });

    private static GoogleImageSearchCommand CreateCommand(string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(Options, TestHelpers.CreateHttpClientFactory(responseJson, statusCode));

    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsEmptySearchTermErrorResult()
    {
        var command = new GoogleImageSearchCommand(Options, new Mock<IHttpClientFactory>().Object);

        var result = await command.ExecuteAsync("");

        result.ShouldBeOfType<EmptySearchTermErrorResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsResults_ReturnsGoogleSearchResult()
    {
        const string json = """
            {
              "searchInformation": { "totalResults": "1" },
              "items": [{ "title": "A cat", "link": "https://example.com/cat.jpg" }]
            }
            """;

        var result = await CreateCommand(json).ExecuteAsync("cat");

        var searchResult = result.ShouldBeOfType<GoogleSearchResult>();
        searchResult.Title.ShouldBe("A cat");
        searchResult.Link.ShouldBe("https://example.com/cat.jpg");
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsZeroTotalResults_ReturnsNoResultsFoundResult()
    {
        const string json = """{ "searchInformation": { "totalResults": "0" } }""";

        var result = await CreateCommand(json).ExecuteAsync("nothing");

        result.ShouldBeOfType<NoResultsFoundResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsError_ReturnsGoogleApiErrorResult()
    {
        const string errorJson = """
            {
              "error": {
                "code": 403,
                "message": "API key expired"
              }
            }
            """;

        var result = await CreateCommand(errorJson, HttpStatusCode.Forbidden).ExecuteAsync("something");

        var errorResult = result.ShouldBeOfType<GoogleApiErrorResult>();
        errorResult.Error.ShouldBe("API key expired");
    }
}