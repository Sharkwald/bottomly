using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class GiphyCommandTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsBadInputResult()
    {
        var mockFactory = new Mock<IHttpClientFactory>();
        var options = Options.Create(new BottomlyOptions { GiphyApiKey = "test" });
        var command = new GiphyCommand(mockFactory.Object, options, NullLogger<GiphyCommand>.Instance);

        var result = await command.ExecuteAsync("");

        result.ShouldBeOfType<GiphyBadInputResult>();
    }

    [Fact]
    public async Task ExecuteAsync_WithResult_ReturnsSuccessResult()
    {
        const string json = """{"data":{"url":"https://giphy.com/gifs/funny-cat"}}""";
        var options = Options.Create(new BottomlyOptions { GiphyApiKey = "test" });
        var command = new GiphyCommand(TestHelpers.CreateHttpClientFactory(json), options, NullLogger<GiphyCommand>.Instance);

        var result = await command.ExecuteAsync("cat");

        var successResult = result.ShouldBeOfType<GiphySuccessResult>();
        successResult.Url.ShouldBe("https://giphy.com/gifs/funny-cat");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDataArray_ReturnsEmptyResult()
    {
        const string json = """{"data":[]}""";
        var options = Options.Create(new BottomlyOptions { GiphyApiKey = "test" });
        var command = new GiphyCommand(TestHelpers.CreateHttpClientFactory(json), options, NullLogger<GiphyCommand>.Instance);

        var result = await command.ExecuteAsync("obscuresearch");

        result.ShouldBeOfType<GiphyEmptyResult>();
    }
}