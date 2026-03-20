using Bottomly.Commands;
using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class AddKarmaCommandTests
{
    private readonly AddKarmaCommand _command;
    private readonly Mock<IKarmaRepository> _mockRepo = new();

    public AddKarmaCommandTests() => _command = new AddKarmaCommand(_mockRepo.Object, NullLogger<AddKarmaCommand>.Instance);

    [Fact]
    public async Task ExecuteAsync_AwardsKarma_PersistsToRepository()
    {
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Karma>())).Returns(Task.CompletedTask);

        var result = await _command.ExecuteAsync("alice", "bob", "great job", KarmaType.PozzyPoz);

        result.ShouldBeOfType<AddKarmaSuccessResult>();
        _mockRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "alice" &&
            k.AwardedByUsername == "bob" &&
            k.Reason == "great job" &&
            k.KarmaType == KarmaType.PozzyPoz)), Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_SelfPositiveKarma_ReturnsSelfAwardResult()
    {
        var result = await _command.ExecuteAsync("alice", "alice", "", KarmaType.PozzyPoz);

        result.ShouldBeOfType<AddKarmaSelfAwardResult>();
    }

    [Fact]
    public async Task ExecuteAsync_SelfNegativeKarma_ReturnsSuccessResult()
    {
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Karma>())).Returns(Task.CompletedTask);

        var result = await _command.ExecuteAsync("alice", "alice", "", KarmaType.NeggyNeg);

        result.ShouldBeOfType<AddKarmaSuccessResult>();
    }

    [Fact]
    public async Task ExecuteAsync_RepositoryThrows_ReturnsErrorResult()
    {
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Karma>())).ThrowsAsync(new Exception("DB error"));

        var result = await _command.ExecuteAsync("alice", "bob", "great job", KarmaType.PozzyPoz);

        var error = result.ShouldBeOfType<AddKarmaErrorResult>();
        error.Error.ShouldBe("DB error");
    }
}