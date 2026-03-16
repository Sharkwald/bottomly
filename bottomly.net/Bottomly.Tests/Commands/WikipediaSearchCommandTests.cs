using Bottomly.Commands;
using Bottomly.Tests.Helpers;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class WikipediaSearchCommandTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsNull()
    {
        var mockFactory = new Mock<IHttpClientFactory>();
        var command = new WikipediaSearchCommand(mockFactory.Object);

        var result = await command.ExecuteAsync("");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithResults_ReturnsTitleAndLink()
    {
        const string json = """["cat",["Cat","Cat (disambiguation)"],["",""],["https://en.wikipedia.org/wiki/Cat","https://en.wikipedia.org/wiki/Cat_(disambiguation)"]]""";
        var command = new WikipediaSearchCommand(TestHelpers.CreateHttpClientFactory(json));

        var result = await command.ExecuteAsync("cat");

        result.ShouldNotBeNull();
        result!.Text.ShouldBe("Cat");
        result.Link.ShouldBe("https://en.wikipedia.org/wiki/Cat");
    }

    [Fact]
    public async Task ExecuteAsync_NoResults_ReturnsNull()
    {
        const string json = """["unknownxyz",[],[],[]]""";
        var command = new WikipediaSearchCommand(TestHelpers.CreateHttpClientFactory(json));

        var result = await command.ExecuteAsync("unknownxyz");

        result.ShouldBeNull();
    }
}