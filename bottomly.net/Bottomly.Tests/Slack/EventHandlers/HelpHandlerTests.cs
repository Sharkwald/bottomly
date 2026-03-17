using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class HelpHandlerTests
{
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();

    private HelpHandler CreateHandler(IEnumerable<IMessageEventHandler>? handlers = null)
    {
        var options = TestHelpers.CreateOptions();
        return new HelpHandler(
            handlers ?? Enumerable.Empty<IMessageEventHandler>(),
            _mockBroker.Object,
            options,
            NullLogger<HelpHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text, string user = "U1") =>
        new() { Text = text, User = user, Channel = "C1", Ts = "ts1" };

    [Theory]
    [InlineData("_help")]
    [InlineData("_?")]
    [InlineData("_list")]
    public void CanHandle_HelpEvent_ReturnsTrue(string text)
    {
        var handler = CreateHandler();
        handler.CanHandle(CreateMessage(text)).ShouldBeTrue();
    }

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse()
    {
        var handler = CreateHandler();
        handler.CanHandle(CreateMessage("hello")).ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_SendsDmWithAllHandlerHelpMessages()
    {
        var mockH1 = new Mock<IMessageEventHandler>();
        mockH1.Setup(h => h.BuildHelpMessage()).Returns("Handler1 Help");
        var mockH2 = new Mock<IMessageEventHandler>();
        mockH2.Setup(h => h.BuildHelpMessage()).Returns("Handler2 Help");

        var handler = CreateHandler(new[] { mockH1.Object, mockH2.Object });
        var message = CreateMessage("_help");

        await handler.HandleAsync(message);

        _mockBroker.Verify(b => b.SendDmAsync(
            It.Is<string>(s => s.Contains("Handler1 Help") && s.Contains("Handler2 Help")),
            "U1"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpForSelf()
    {
        var handler = CreateHandler();

        await handler.HandleAsync(CreateMessage("_help -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Help")), "C1", null), Times.Once());
    }
}