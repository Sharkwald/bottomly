using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Slack;
using Bottomly.Slack.EventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class ReleaseEventHandlerTests
{
    private readonly ReleaseEventHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<ReleaseCommand> _mockCommand;

    public ReleaseEventHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        var releaseOptions = Options.Create(new BottomlyOptions { GitHubToken = "token" });
        _mockCommand = new Mock<ReleaseCommand>(releaseOptions, NullLogger<ReleaseCommand>.Instance);
        _handler = new ReleaseEventHandler(_mockCommand.Object, _mockBroker.Object, options,
            NullLogger<ReleaseEventHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue() => _handler.CanHandle(CreateMessage("_release")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() => _handler.CanHandle(CreateMessage("hello")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_WithResult_SendsResult()
    {
        _mockCommand.Setup(c => c.ExecuteAsync()).ReturnsAsync("Latest Release: *v1.0* v1.0");

        await _handler.HandleAsync(CreateMessage("_release"));

        _mockBroker.Verify(b => b.SendMessageAsync("Latest Release: *v1.0* v1.0", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_NullResult_SendsUnableMessage()
    {
        _mockCommand.Setup(c => c.ExecuteAsync()).ReturnsAsync((string?)null);

        await _handler.HandleAsync(CreateMessage("_release"));

        _mockBroker.Verify(b => b.SendMessageAsync("Unable to retrieve latest release info.", "C1", null),
            Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_release -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Release")), "C1", null),
            Times.Once());
    }
}