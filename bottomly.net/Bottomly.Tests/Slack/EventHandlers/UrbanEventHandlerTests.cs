using Bottomly.Commands;
using Bottomly.Slack;
using Bottomly.Slack.EventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class UrbanEventHandlerTests
{
    private readonly UrbanEventHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<UrbanSearchCommand> _mockCommand;

    public UrbanEventHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        var mockFactory = new Mock<IHttpClientFactory>();
        _mockCommand = new Mock<UrbanSearchCommand>(mockFactory.Object);
        _handler = new UrbanEventHandler(_mockCommand.Object, _mockBroker.Object, options,
            NullLogger<UrbanEventHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue() => _handler.CanHandle(CreateMessage("_ud whatever")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() => _handler.CanHandle(CreateMessage("no prefix")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_ValidEvent_CallsCommandWithTerm()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("hello")).ReturnsAsync((string?)null);

        await _handler.HandleAsync(CreateMessage("_ud hello"));

        _mockCommand.Verify(c => c.ExecuteAsync("hello"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_WithResult_SendsReplyResponse()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("hello")).ReturnsAsync("a greeting");
        var message = CreateMessage("_ud hello");

        await _handler.HandleAsync(message);

        // asReply:true means threadTs = message.Ts = "ts1"
        _mockBroker.Verify(b => b.SendMessageAsync("a greeting", "C1", "ts1"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_NullResult_SendsExerciseMessage()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("xyz")).ReturnsAsync((string?)null);

        await _handler.HandleAsync(CreateMessage("_ud xyz"));

        _mockBroker.Verify(b => b.SendMessageAsync("Left as an exercise for the reader.", "C1", "ts1"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_ud -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Urban Dictionary")), "C1", null),
            Times.Once());
    }
}