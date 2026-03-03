using Bottomly.Commands;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class GetLoserBoardHandlerTests
{
    private readonly GetLoserBoardHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<IKarmaRepository> _mockKarmaRepo = new();

    public GetLoserBoardHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        var command = new GetLoserBoardCommand(_mockKarmaRepo.Object);
        _handler = new GetLoserBoardHandler(command, _mockBroker.Object, options,
            NullLogger<GetLoserBoardHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEventNoSize_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_loserboard")).ShouldBeTrue();

    [Fact]
    public void CanHandle_ValidEventWithSize_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_loserboard 5")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() => _handler.CanHandle(CreateMessage("hello")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_NoSize_CallsCommandWithDefaultSize3()
    {
        _mockKarmaRepo.Setup(r => r.GetLoserBoardAsync(3)).ReturnsAsync(new List<KarmaScore>().AsReadOnly());

        await _handler.HandleAsync(CreateMessage("_loserboard "));

        _mockKarmaRepo.Verify(r => r.GetLoserBoardAsync(3), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithSize10_CallsCommandWithSize10()
    {
        _mockKarmaRepo.Setup(r => r.GetLoserBoardAsync(10)).ReturnsAsync(new List<KarmaScore>().AsReadOnly());

        await _handler.HandleAsync(CreateMessage("_loserboard 10"));

        _mockKarmaRepo.Verify(r => r.GetLoserBoardAsync(10), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSize_CallsCommandWithDefaultSize3()
    {
        _mockKarmaRepo.Setup(r => r.GetLoserBoardAsync(3)).ReturnsAsync(new List<KarmaScore>().AsReadOnly());

        await _handler.HandleAsync(CreateMessage("_loserboard abc"));

        _mockKarmaRepo.Verify(r => r.GetLoserBoardAsync(3), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithSize0_CallsCommandWithDefaultSize3()
    {
        _mockKarmaRepo.Setup(r => r.GetLoserBoardAsync(3)).ReturnsAsync(new List<KarmaScore>().AsReadOnly());

        await _handler.HandleAsync(CreateMessage("_loserboard 0"));

        _mockKarmaRepo.Verify(r => r.GetLoserBoardAsync(3), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithResult_SendsFormattedResponse()
    {
        var scores = new List<KarmaScore> { new("name1", -2), new("name2", -5) }.AsReadOnly();
        _mockKarmaRepo.Setup(r => r.GetLoserBoardAsync(3)).ReturnsAsync(scores);

        await _handler.HandleAsync(CreateMessage("_loserboard "));

        _mockBroker.Verify(b => b.SendMessageAsync(
            $"name1: -2{Environment.NewLine}name2: -5",
            "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_loserboard -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Get Loserboard")), "C1", null),
            Times.Once());
    }
}