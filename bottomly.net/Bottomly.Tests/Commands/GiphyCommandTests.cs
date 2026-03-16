using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class GiphyCommandTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsNull()
    {
        var mockFactory = new Mock<IHttpClientFactory>();
        var options = Options.Create(new BottomlyOptions { GiphyApiKey = "test" });
        var command = new GiphyCommand(mockFactory.Object, options);

        var result = await command.ExecuteAsync("");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithResult_ReturnsUrl()
    {
        const string json = """{"data":{"url":"https://giphy.com/gifs/funny-cat"}}""";
        var options = Options.Create(new BottomlyOptions { GiphyApiKey = "test" });
        var command = new GiphyCommand(TestHelpers.CreateHttpClientFactory(json), options);

        var result = await command.ExecuteAsync("cat");

        result.ShouldBe("https://giphy.com/gifs/funny-cat");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDataArray_ReturnsNull()
    {
        const string json = """{"data":[]}""";
        var options = Options.Create(new BottomlyOptions { GiphyApiKey = "test" });
        var command = new GiphyCommand(TestHelpers.CreateHttpClientFactory(json), options);

        var result = await command.ExecuteAsync("obscuresearch");

        result.ShouldBeNull();
    }
}