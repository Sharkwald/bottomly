using Bottomly.Commands;
using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers;

public class GetCurrentKarmaReasonsHandlerTests
{
    private readonly GetCurrentKarmaReasonsHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<IKarmaRepository> _mockKarmaRepo = new();
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();

    public GetCurrentKarmaReasonsHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        var command = new GetCurrentKarmaReasonsCommand(_mockKarmaRepo.Object);
        var parser = new SlackParser(_mockMemberRepo.Object);
        _handler = new GetCurrentKarmaReasonsHandler(command, parser, _mockBroker.Object, options,
            NullLogger<GetCurrentKarmaReasonsHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text, string user = "U_sender") =>
        new() { Text = text, User = user, Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_WithRecipient_ReturnsTrue() =>
        _handler.CanHandle(CreateMessage("_reasons alice")).ShouldBeTrue();

    [Fact]
    public void CanHandle_NoRecipient_ReturnsTrue() => _handler.CanHandle(CreateMessage("_reasons")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() =>
        _handler.CanHandle(CreateMessage("hello world")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_PlainRecipient_CallsCommandWithRecipient()
    {
        _mockKarmaRepo.Setup(r => r.GetKarmaReasonsAsync("alice")).ReturnsAsync(
            new KarmaReasonsResult(0, new List<Karma>().AsReadOnly()));

        await _handler.HandleAsync(CreateMessage("_reasons alice"));

        _mockKarmaRepo.Verify(r => r.GetKarmaReasonsAsync("alice"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_NoRecipient_CallsCommandWithMessageUser()
    {
        _mockKarmaRepo.Setup(r => r.GetKarmaReasonsAsync("U_sender")).ReturnsAsync(
            new KarmaReasonsResult(0, new List<Karma>().AsReadOnly()));

        await _handler.HandleAsync(CreateMessage("_reasons "));

        _mockKarmaRepo.Verify(r => r.GetKarmaReasonsAsync("U_sender"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_WithResult_SendsFormattedResponse()
    {
        var reasoned = new List<Karma>
        {
            new()
            {
                AwardedByUsername = "bob", AwardedToUsername = "alice", Reason = "great work",
                KarmaType = KarmaType.PozzyPoz
            },
            new()
            {
                AwardedByUsername = "carol", AwardedToUsername = "alice", Reason = "bad work",
                KarmaType = KarmaType.NeggyNeg
            }
        }.AsReadOnly();
        _mockKarmaRepo.Setup(r => r.GetKarmaReasonsAsync("alice")).ReturnsAsync(new KarmaReasonsResult(1, reasoned));

        await _handler.HandleAsync(CreateMessage("_reasons alice"));

        _mockBroker.Verify(b => b.SendDmAsync(
            It.Is<string>(s =>
                s.Contains("Recent Karma for alice") &&
                s.Contains("Recently awarded with no reason: 1") &&
                s.Contains("++ from bob for \"great work\"") &&
                s.Contains("-- from carol for \"bad work\"")),
            "U_sender"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        await _handler.HandleAsync(CreateMessage("_reasons -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Karma Reasons")), "C1", null),
            Times.Once());
    }
}