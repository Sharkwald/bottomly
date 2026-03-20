using Bottomly.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands.Integration;

/// <summary>
///     Integration tests that call the real Wikipedia API.
///     These tests expose issues with the HTTP call format (e.g. missing headers, wrong URL structure)
///     that mocked unit tests cannot detect.
/// </summary>
public class WikipediaSearchCommandIntegrationTests
{
    private readonly WikipediaSearchCommand _sut;

    public WikipediaSearchCommandIntegrationTests()
    {
        var client = new HttpClient();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        _sut = new WikipediaSearchCommand(factory.Object, NullLogger<WikipediaSearchCommand>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsEmptyInputResult()
    {
        var result = await _sut.ExecuteAsync("");

        result.ShouldBeOfType<WikipediaEmptyInputResult>();
    }

    [Fact]
    public async Task ExecuteAsync_KnownSearchTerm_ReturnsSuccessResultWithWikipediaLink()
    {
        var result = await _sut.ExecuteAsync("Albert Einstein");

        var success = result.ShouldBeOfType<WikipediaSuccessResult>();
        success.Text.ShouldNotBeNullOrEmpty();
        success.Link.ShouldStartWith("https://en.wikipedia.org/wiki/");
    }

    [Fact]
    public async Task ExecuteAsync_KnownSearchTerm_ReturnsExpectedTitle()
    {
        var result = await _sut.ExecuteAsync("London");

        var success = result.ShouldBeOfType<WikipediaSuccessResult>();
        success.Text.ShouldBe("London");
    }

    [Fact]
    public async Task ExecuteAsync_GibberishInput_ReturnsNotFoundResult()
    {
        var result = await _sut.ExecuteAsync("xyzzy_no_such_article_12345");

        result.ShouldBeOfType<WikipediaNotFoundResult>();
    }
}