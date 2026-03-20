using Bottomly.Commands;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class WikipediaSearchCommandTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsEmptyInputResult()
    {
        var mockFactory = new Mock<IHttpClientFactory>();
        var command = new WikipediaSearchCommand(mockFactory.Object, NullLogger<WikipediaSearchCommand>.Instance);

        var result = await command.ExecuteAsync("");

        result.ShouldBeOfType<WikipediaEmptyInputResult>();
    }

    [Fact]
    public async Task ExecuteAsync_WithResults_ReturnsSuccessResult()
    {
        const string json =
            """["cat",["Cat","Cat (disambiguation)"],["",""],["https://en.wikipedia.org/wiki/Cat","https://en.wikipedia.org/wiki/Cat_(disambiguation)"]]""";
        var command = new WikipediaSearchCommand(TestHelpers.CreateHttpClientFactory(json), NullLogger<WikipediaSearchCommand>.Instance);

        var result = await command.ExecuteAsync("cat");

        var success = result.ShouldBeOfType<WikipediaSuccessResult>();
        success.Text.ShouldBe("Cat");
        success.Link.ShouldBe("https://en.wikipedia.org/wiki/Cat");
    }

    [Fact]
    public async Task ExecuteAsync_NoResults_ReturnsNotFoundResult()
    {
        const string json = """["unknownxyz",[],[],[]]""";
        var command = new WikipediaSearchCommand(TestHelpers.CreateHttpClientFactory(json), NullLogger<WikipediaSearchCommand>.Instance);

        var result = await command.ExecuteAsync("unknownxyz");

        result.ShouldBeOfType<WikipediaNotFoundResult>();
    }
}