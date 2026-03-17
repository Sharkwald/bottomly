using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class RegHandlerTests
{
    private readonly RegHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<RegSearchCommand> _mockCommand;

    public RegHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        _mockCommand = new Mock<RegSearchCommand>(new Mock<IHttpClientFactory>().Object);
        _handler = new RegHandler(_mockCommand.Object, _mockBroker.Object, options,
            NullLogger<RegHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_reg AB12CDE")).ShouldBeTrue();

    [Fact]
    public void CanHandle_JustTrigger_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_reg")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() =>
        _handler.CanHandle(CreateMessage("hello")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_WithResult_SendsResult()
    {
        _mockCommand.Setup(c => c.ExecuteAsync(It.IsAny<string>()))
            .ReturnsAsync("Red Ford Focus (2019)");

        await _handler.HandleAsync(CreateMessage("_reg AB12CDE"));

        _mockBroker.Verify(b => b.SendMessageAsync("Red Ford Focus (2019)", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_reg -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(
            It.Is<string>(s => s.Contains("Reg Lookup")), "C1", null), Times.Once());
    }
}

public class TestHandlerTests
{
    private readonly TestHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();

    public TestHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        _handler = new TestHandler(_mockBroker.Object, options, NullLogger<TestHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "U1", Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_test")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() =>
        _handler.CanHandle(CreateMessage("hello")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_SendsOkMessage()
    {
        await _handler.HandleAsync(CreateMessage("_test"));

        _mockBroker.Verify(b => b.SendMessageAsync("OK", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_test -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(
            It.Is<string>(s => s.Contains("Test")), "C1", null), Times.Once());
    }
}
