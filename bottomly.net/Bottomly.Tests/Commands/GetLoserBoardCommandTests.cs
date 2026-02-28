using Bottomly.Commands;
using Bottomly.Repositories;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class GetLoserBoardCommandTests
{
    private readonly GetLoserBoardCommand _command;
    private readonly Mock<IKarmaRepository> _mockRepo = new();

    public GetLoserBoardCommandTests() => _command = new GetLoserBoardCommand(_mockRepo.Object);

    [Fact]
    public async Task ExecuteAsync_DefaultSize_CallsRepositoryWithSize3()
    {
        var scores = new List<KarmaScore> { new("alice", -5) }.AsReadOnly();
        _mockRepo.Setup(r => r.GetLoserBoardAsync(3)).ReturnsAsync(scores);

        var result = await _command.ExecuteAsync();

        result.ShouldBe(scores);
        _mockRepo.Verify(r => r.GetLoserBoardAsync(3), Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_SpecifiedSize_CallsRepositoryWithSpecifiedSize()
    {
        var scores = new List<KarmaScore> { new("alice", -5) }.AsReadOnly();
        _mockRepo.Setup(r => r.GetLoserBoardAsync(10)).ReturnsAsync(scores);

        var result = await _command.ExecuteAsync(10);

        result.ShouldBe(scores);
        _mockRepo.Verify(r => r.GetLoserBoardAsync(10), Times.Once());
    }
}