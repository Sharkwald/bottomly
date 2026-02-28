using Bottomly.Commands;
using Bottomly.Models;
using Bottomly.Repositories;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class AddKarmaCommandTests
{
    private readonly AddKarmaCommand _command;
    private readonly Mock<IKarmaRepository> _mockRepo = new();

    public AddKarmaCommandTests() => _command = new AddKarmaCommand(_mockRepo.Object);

    [Fact]
    public async Task ExecuteAsync_AwardsKarma_PersistsToRepository()
    {
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Karma>())).Returns(Task.CompletedTask);

        await _command.ExecuteAsync("alice", "bob", "great job", KarmaType.PozzyPoz);

        _mockRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "alice" &&
            k.AwardedByUsername == "bob" &&
            k.Reason == "great job" &&
            k.KarmaType == KarmaType.PozzyPoz)), Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_SelfPositiveKarma_ThrowsInvalidOperation() =>
        await Should.ThrowAsync<InvalidOperationException>(() =>
            _command.ExecuteAsync("alice", "alice", "", KarmaType.PozzyPoz));

    [Fact]
    public async Task ExecuteAsync_SelfNegativeKarma_DoesNotThrow()
    {
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Karma>())).Returns(Task.CompletedTask);

        await Should.NotThrowAsync(() => _command.ExecuteAsync("alice", "alice", "", KarmaType.NeggyNeg));
    }
}