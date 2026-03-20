using Bottomly.Commands.Search;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class SearchHandlerTests
{
    private readonly SearchHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<SearchCommand> _mockCommand;

    public SearchHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        _mockCommand = new Mock<SearchCommand>(options, NullLogger<SearchCommand>.Instance,
            new Mock<IHttpClientFactory>().Object);
        _handler = new SearchHandler(_mockCommand.Object, _mockBroker.Object, options,
            NullLogger<SearchHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_g a valid google command")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() =>
        _handler.CanHandle(CreateMessage("no prefix here")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_ValidEvent_CallsCommandWithQuery()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("some query")).ReturnsAsync(new NoResultsFoundResult());

        await _handler.HandleAsync(CreateMessage("_g some query"));

        _mockCommand.Verify(c => c.ExecuteAsync("some query"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_WithResult_SendsFormattedResponse()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("dotnet"))
            .ReturnsAsync(new SearchResult("DotNet", "https://dotnet.microsoft.com"));

        await _handler.HandleAsync(CreateMessage("_g dotnet"));

        _mockBroker.Verify(b => b.SendMessageAsync("<https://dotnet.microsoft.com|DotNet>", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_EmptySearchTermResult_SendsNoResultMessage()
    {
        _mockCommand.Setup(c => c.ExecuteAsync("xyz")).ReturnsAsync(new EmptySearchTermErrorResult());

        await _handler.HandleAsync(CreateMessage("_g xyz"));

        _mockBroker.Verify(b => b.SendMessageAsync("No results found for \"xyz\"", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_g -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Web Search")), "C1", null),
            Times.Once());
    }
}