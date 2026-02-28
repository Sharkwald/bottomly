using Bottomly.Commands;
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
}