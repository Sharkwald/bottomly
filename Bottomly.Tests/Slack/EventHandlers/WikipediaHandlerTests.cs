using Bottomly.Commands;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class WikipediaHandlerTests
{
    private readonly WikipediaHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<WikipediaSearchCommand> _mockCommand;

    public WikipediaHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        var mockFactory = new Mock<IHttpClientFactory>();
        _mockCommand = new Mock<WikipediaSearchCommand>(mockFactory.Object, NullLogger<WikipediaSearchCommand>.Instance);
        _handler = new WikipediaHandler(_mockCommand.Object, _mockBroker.Object, options,
            NullLogger<WikipediaHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue() => _handler.CanHandle(CreateMessage("_wik octopus")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() => _handler.CanHandle(CreateMessage("no prefix")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_ValidEvent_CallsCommandWithTerm()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("octopus")).ReturnsAsync(new WikipediaNotFoundResult());

        await _handler.HandleAsync(CreateMessage("_wik octopus"));

        _mockCommand.Verify(c => c.ExecuteAsync("octopus"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_WithResult_SendsFormattedResponse()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("octopus"))
            .ReturnsAsync(new WikipediaSuccessResult("Octopus", "https://en.wikipedia.org/wiki/Octopus"));

        await _handler.HandleAsync(CreateMessage("_wik octopus"));

        _mockBroker.Verify(b => b.SendMessageAsync("<https://en.wikipedia.org/wiki/Octopus|Octopus>", "C1", null),
            Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_NotFoundResult_SendsNoResultMessage()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("xyz")).ReturnsAsync(new WikipediaNotFoundResult());

        await _handler.HandleAsync(CreateMessage("_wik xyz"));

        _mockBroker.Verify(b => b.SendMessageAsync("No results found for \"xyz\"", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_wik -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Wikipedia")), "C1", null),
            Times.Once());
    }
}