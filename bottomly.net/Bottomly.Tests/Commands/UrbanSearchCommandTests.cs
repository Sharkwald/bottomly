using Bottomly.Commands;
using Bottomly.Tests.Helpers;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class UrbanSearchCommandTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsNull()
    {
        var mockFactory = new Mock<IHttpClientFactory>();
        var command = new UrbanSearchCommand(mockFactory.Object);

        var result = await command.ExecuteAsync("");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithResults_ReturnsDefinition()
    {
        const string json = """{"list":[{"definition":"A domestic animal that owns you."}]}""";
        var command = new UrbanSearchCommand(TestHelpers.CreateHttpClientFactory(json));

        var result = await command.ExecuteAsync("cat");

        result.ShouldBe("A domestic animal that owns you.");
    }

    [Fact]
    public async Task ExecuteAsync_NoResults_ReturnsNull()
    {
        const string json = """{"list":[]}""";
        var command = new UrbanSearchCommand(TestHelpers.CreateHttpClientFactory(json));

        var result = await command.ExecuteAsync("xyznotaword");

        result.ShouldBeNull();
    }
}