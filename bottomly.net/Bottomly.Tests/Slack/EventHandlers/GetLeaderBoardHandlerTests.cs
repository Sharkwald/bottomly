using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class GetLeaderBoardHandlerTests
{
    private readonly GetLeaderBoardHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<IKarmaRepository> _mockKarmaRepo = new();

    public GetLeaderBoardHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        _handler = new GetLeaderBoardHandler(_mockKarmaRepo.Object, _mockBroker.Object, options,
            NullLogger<GetLeaderBoardHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEventNoSize_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_leaderboard")).ShouldBeTrue();

    [Fact]
    public void CanHandle_ValidEventWithSize_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_leaderboard 5")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() => _handler.CanHandle(CreateMessage("hello")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_NoSize_CallsCommandWithDefaultSize3()
    {
        _mockKarmaRepo.Setup(r => r.GetLeaderBoardAsync(3)).ReturnsAsync(new List<KarmaScore>().AsReadOnly());

        await _handler.HandleAsync(CreateMessage("_leaderboard "));

        _mockKarmaRepo.Verify(r => r.GetLeaderBoardAsync(3), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithSize10_CallsCommandWithSize10()
    {
        _mockKarmaRepo.Setup(r => r.GetLeaderBoardAsync(10)).ReturnsAsync(new List<KarmaScore>().AsReadOnly());

        await _handler.HandleAsync(CreateMessage("_leaderboard 10"));

        _mockKarmaRepo.Verify(r => r.GetLeaderBoardAsync(10), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSize_CallsCommandWithDefaultSize3()
    {
        _mockKarmaRepo.Setup(r => r.GetLeaderBoardAsync(3)).ReturnsAsync(new List<KarmaScore>().AsReadOnly());

        await _handler.HandleAsync(CreateMessage("_leaderboard abc"));

        _mockKarmaRepo.Verify(r => r.GetLeaderBoardAsync(3), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithSize0_CallsCommandWithDefaultSize3()
    {
        _mockKarmaRepo.Setup(r => r.GetLeaderBoardAsync(3)).ReturnsAsync(new List<KarmaScore>().AsReadOnly());

        await _handler.HandleAsync(CreateMessage("_leaderboard 0"));

        _mockKarmaRepo.Verify(r => r.GetLeaderBoardAsync(3), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithResult_SendsFormattedResponse()
    {
        var scores = new List<KarmaScore> { new("name1", 2), new("name2", 1) }.AsReadOnly();
        _mockKarmaRepo.Setup(r => r.GetLeaderBoardAsync(3)).ReturnsAsync(scores);

        await _handler.HandleAsync(CreateMessage("_leaderboard "));

        _mockBroker.Verify(b => b.SendMessageAsync(
            $"name1: 2{Environment.NewLine}name2: 1",
            "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_leaderboard -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Get Leaderboard")), "C1", null),
            Times.Once());
    }
}