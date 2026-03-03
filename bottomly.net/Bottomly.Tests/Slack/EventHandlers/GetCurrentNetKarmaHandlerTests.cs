using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class GetCurrentNetKarmaHandlerTests
{
    private readonly GetCurrentNetKarmaHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<IKarmaRepository> _mockKarmaRepo = new();
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();

    public GetCurrentNetKarmaHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        var parser = new SlackParser(_mockMemberRepo.Object);
        _handler = new GetCurrentNetKarmaHandler(_mockKarmaRepo.Object, parser, _mockBroker.Object, options,
            NullLogger<GetCurrentNetKarmaHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text, string user = "U_sender") =>
        new() { Text = text, User = user, Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_WithPlainRecipient_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_karma recipient")).ShouldBeTrue();

    [Fact]
    public void CanHandle_WithSlackIdRecipient_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_karma <@U12345>")).ShouldBeTrue();

    [Fact]
    public void CanHandle_NoRecipient_ReturnsTrue() => _handler.CanHandle(CreateMessage("_karma")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() =>
        _handler.CanHandle(CreateMessage("hello world")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_PlainRecipient_CallsCommandWithRecipient()
    {
        _mockKarmaRepo.Setup(r => r.GetCurrentNetKarmaAsync("alice")).ReturnsAsync(3);

        await _handler.HandleAsync(CreateMessage("_karma alice"));

        _mockKarmaRepo.Verify(r => r.GetCurrentNetKarmaAsync("alice"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_NoRecipient_CallsCommandWithMessageUser()
    {
        _mockKarmaRepo.Setup(r => r.GetCurrentNetKarmaAsync("U_sender")).ReturnsAsync(0);

        await _handler.HandleAsync(CreateMessage("_karma "));

        _mockKarmaRepo.Verify(r => r.GetCurrentNetKarmaAsync("U_sender"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithResult_SendsFormattedResponse()
    {
        _mockKarmaRepo.Setup(r => r.GetCurrentNetKarmaAsync("alice")).ReturnsAsync(0);

        await _handler.HandleAsync(CreateMessage("_karma alice"));

        _mockBroker.Verify(b => b.SendMessageAsync("alice: 0", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_karma -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Get Current Karma")), "C1", null),
            Times.Once());
    }
}