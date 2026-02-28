using Bottomly.Commands;
using Bottomly.Models;
using Bottomly.Repositories;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class GetCurrentKarmaReasonsCommandTests
{
    [Fact]
    public async Task ExecuteAsync_DelegatesToRepository()
    {
        var mockRepo = new Mock<IKarmaRepository>();
        var expected = new KarmaReasonsResult(2, new List<Karma>().AsReadOnly());
        mockRepo.Setup(r => r.GetKarmaReasonsAsync("alice")).ReturnsAsync(expected);
        var command = new GetCurrentKarmaReasonsCommand(mockRepo.Object);

        var result = await command.ExecuteAsync("alice");

        result.ShouldBe(expected);
        mockRepo.Verify(r => r.GetKarmaReasonsAsync("alice"), Times.Once());
    }
}