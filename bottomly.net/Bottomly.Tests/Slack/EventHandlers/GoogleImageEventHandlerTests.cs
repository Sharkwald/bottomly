using Bottomly.Commands;
using Bottomly.Slack;
using Bottomly.Slack.EventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class GoogleImageEventHandlerTests
{
    private readonly GoogleImageEventHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<GoogleImageSearchCommand> _mockCommand;

    public GoogleImageEventHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        _mockCommand = new Mock<GoogleImageSearchCommand>(options);
        _handler = new GoogleImageEventHandler(_mockCommand.Object, _mockBroker.Object, options,
            NullLogger<GoogleImageEventHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue() => _handler.CanHandle(CreateMessage("_gi cats")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() =>
        _handler.CanHandle(CreateMessage("no prefix here")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_ValidEvent_CallsCommandWithQuery()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("cats")).ReturnsAsync((GoogleSearchResult?)null);

        await _handler.HandleAsync(CreateMessage("_gi cats"));

        _mockCommand.Verify(c => c.ExecuteAsync("cats"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_WithResult_SendsFormattedResponse()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("cats"))
            .ReturnsAsync(new GoogleSearchResult("Cute Cat", "https://example.com/cat.jpg"));

        await _handler.HandleAsync(CreateMessage("_gi cats"));

        _mockBroker.Verify(b => b.SendMessageAsync("Cute Cat https://example.com/cat.jpg", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_NullResult_SendsNoResultMessage()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("xyz")).ReturnsAsync((GoogleSearchResult?)null);

        await _handler.HandleAsync(CreateMessage("_gi xyz"));

        _mockBroker.Verify(b => b.SendMessageAsync("No image results found for \"xyz\"", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_gi -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Google Image")), "C1", null),
            Times.Once());
    }
}