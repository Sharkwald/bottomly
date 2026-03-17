using Bottomly.Commands;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class GiphyHandlerTests
{
    private readonly GiphyHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<GiphyCommand> _mockCommand;

    public GiphyHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        var mockFactory = new Mock<IHttpClientFactory>();
        _mockCommand = new Mock<GiphyCommand>(mockFactory.Object, options);
        _handler = new GiphyHandler(_mockCommand.Object, _mockBroker.Object, options,
            NullLogger<GiphyHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue() => _handler.CanHandle(CreateMessage("_gif cats")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() => _handler.CanHandle(CreateMessage("no prefix")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_ValidEvent_CallsCommandWithTerm()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("cats")).ReturnsAsync((string?)null);

        await _handler.HandleAsync(CreateMessage("_gif cats"));

        _mockCommand.Verify(c => c.ExecuteAsync("cats"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_WithResult_SendsResult()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("cats")).ReturnsAsync("https://giphy.com/cat.gif");

        await _handler.HandleAsync(CreateMessage("_gif cats"));

        _mockBroker.Verify(b => b.SendMessageAsync("https://giphy.com/cat.gif", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_NullResult_SendsNoGifsMessage()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("xyz")).ReturnsAsync((string?)null);

        await _handler.HandleAsync(CreateMessage("_gif xyz"));

        _mockBroker.Verify(b => b.SendMessageAsync("No gifs found for \"xyz\"", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_gif -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Giphy")), "C1", null), Times.Once());
    }
}