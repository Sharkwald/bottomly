using Bottomly.Commands;
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
}