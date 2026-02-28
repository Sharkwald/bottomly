using Bottomly.Commands;
using Bottomly.Configuration;
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
}