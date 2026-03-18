using Bottomly.Commands.Search;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class ImageSearchHandlerTests
{
    private readonly ImageSearchHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<ImageSearchCommand> _mockCommand;

    public ImageSearchHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        _mockCommand = new Mock<ImageSearchCommand>(options, new Mock<IHttpClientFactory>().Object,
            NullLogger<ImageSearchCommand>.Instance);
        _handler = new ImageSearchHandler(_mockCommand.Object, _mockBroker.Object, options,
            NullLogger<ImageSearchHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text)
    {
        return new MessageEvent { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };
    }

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue()
    {
        _handler.CanHandle(CreateMessage("_gi cats")).ShouldBeTrue();
    }

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse()
    {
        _handler.CanHandle(CreateMessage("no prefix here")).ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_CallsCommandWithQuery()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("cats")).ReturnsAsync(new NoResultsFoundResult());

        await _handler.HandleAsync(CreateMessage("_gi cats"));

        _mockCommand.Verify(c => c.ExecuteAsync("cats"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_WithResult_SendsFormattedResponse()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("cats"))
            .ReturnsAsync(new SearchResult("Cute Cat", "https://example.com/cat.jpg"));

        await _handler.HandleAsync(CreateMessage("_gi cats"));

        _mockBroker.Verify(b => b.SendMessageAsync("Cute Cat https://example.com/cat.jpg", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_NoResultsFound_SendsNoResultMessage()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("xyz")).ReturnsAsync(new NoResultsFoundResult());

        await _handler.HandleAsync(CreateMessage("_gi xyz"));

        _mockBroker.Verify(b => b.SendMessageAsync("No image results found for \"xyz\"", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_gi -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Image Search")), "C1", null),
            Times.Once());
    }
}