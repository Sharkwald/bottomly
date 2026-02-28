using Bottomly.Commands;
using Bottomly.Repositories;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class GetCurrentNetKarmaCommandTests
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToRepository()
    {
        var mockRepo = new Mock<IKarmaRepository>();
        mockRepo.Setup(r => r.GetCurrentNetKarmaAsync("alice")).ReturnsAsync(5);
        var command = new GetCurrentNetKarmaCommand(mockRepo.Object);

        var result = await command.ExecuteAsync("alice");

        result.ShouldBe(5);
        mockRepo.Verify(r => r.GetCurrentNetKarmaAsync("alice"), Times.Once());
    }
}