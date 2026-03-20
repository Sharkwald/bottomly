using Bottomly.Commands;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class UrbanSearchCommandTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsEmptyInputResult()
    {
        var mockFactory = new Mock<IHttpClientFactory>();
        var command = new UrbanSearchCommand(mockFactory.Object, NullLogger<UrbanSearchCommand>.Instance);

        var result = await command.ExecuteAsync("");

        result.ShouldBeOfType<UrbanEmptyInputResult>();
    }

    [Fact]
    public async Task ExecuteAsync_WithResults_ReturnsSuccessResult()
    {
        const string json = """{"list":[{"definition":"A domestic animal that owns you."}]}""";
        var command = new UrbanSearchCommand(TestHelpers.CreateHttpClientFactory(json), NullLogger<UrbanSearchCommand>.Instance);

        var result = await command.ExecuteAsync("cat");

        var success = result.ShouldBeOfType<UrbanSuccessResult>();
        success.Definition.ShouldBe("A domestic animal that owns you.");
    }

    [Fact]
    public async Task ExecuteAsync_NoResults_ReturnsNotFoundResult()
    {
        const string json = """{"list":[]}""";
        var command = new UrbanSearchCommand(TestHelpers.CreateHttpClientFactory(json), NullLogger<UrbanSearchCommand>.Instance);

        var result = await command.ExecuteAsync("xyznotaword");

        result.ShouldBeOfType<UrbanNotFoundResult>();
    }
}