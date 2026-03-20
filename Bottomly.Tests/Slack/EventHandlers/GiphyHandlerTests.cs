using Bottomly.Commands;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Blocks;
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
        _mockCommand = new Mock<GiphyCommand>(mockFactory.Object, options, NullLogger<GiphyCommand>.Instance);
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
        _mockCommand.Setup(c => c.ExecuteAsync("cats")).ReturnsAsync(new GiphyEmptyResult());

        await _handler.HandleAsync(CreateMessage("_gif cats"));

        _mockCommand.Verify(c => c.ExecuteAsync("cats"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_WithResult_SendsImageBlock()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("cats"))
            .ReturnsAsync(new GiphySuccessResult("https://giphy.com/cat.gif"));

        IReadOnlyList<Block>? capturedBlocks = null;
        _mockBroker
            .Setup(b => b.SendBlocksMessageAsync(It.IsAny<IReadOnlyList<Block>>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>()))
            .Callback<IReadOnlyList<Block>, string, string?, string?>((blocks, _, _, _) => capturedBlocks = blocks)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(CreateMessage("_gif cats"));

        _mockBroker.Verify(b => b.SendBlocksMessageAsync(
            It.IsAny<IReadOnlyList<Block>>(), "C1", "cats", null), Times.Once());
        capturedBlocks.ShouldNotBeNull();
        capturedBlocks.Count.ShouldBe(1);
        var imageBlock = capturedBlocks[0].ShouldBeOfType<ImageBlock>();
        imageBlock.ImageUrl.ShouldBe("https://giphy.com/cat.gif");
        imageBlock.AltText.ShouldBe("cats");
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_EmptyResult_SendsNoGifsMessage()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("xyz")).ReturnsAsync(new GiphyEmptyResult());

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