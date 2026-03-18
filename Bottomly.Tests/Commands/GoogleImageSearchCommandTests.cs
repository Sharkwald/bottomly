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

    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsNull()
    {
        var command = new GoogleImageSearchCommand(Options, new Mock<IHttpClientFactory>().Object);

        var result = await command.ExecuteAsync("");

        result.ShouldBeNull();
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
        var command = new GoogleImageSearchCommand(Options, TestHelpers.CreateHttpClientFactory(json));

        var result = await command.ExecuteAsync("cat");

        result.ShouldNotBeNull();
        result!.Title.ShouldBe("A cat");
        result.Link.ShouldBe("https://example.com/cat.jpg");
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsZeroTotalResults_ReturnsNull()
    {
        const string json = """{ "searchInformation": { "totalResults": "0" } }""";
        var command = new GoogleImageSearchCommand(Options, TestHelpers.CreateHttpClientFactory(json));

        var result = await command.ExecuteAsync("nothing");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ApiReturnsError_ReturnsNull()
    {
        var command = new GoogleImageSearchCommand(Options,
            TestHelpers.CreateHttpClientFactory("{}", HttpStatusCode.Forbidden));

        var result = await command.ExecuteAsync("something");

        result.ShouldBeNull();
    }
}