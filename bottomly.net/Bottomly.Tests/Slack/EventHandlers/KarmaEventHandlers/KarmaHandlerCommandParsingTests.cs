using Bottomly.Commands;
using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers.KarmaEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers.KarmaEventHandlers;

public class KarmaHandlerCommandParsingTests
{
    private readonly IncrementMessageKarmaEventHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<IKarmaRepository> _mockKarmaRepo = new();
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();

    public KarmaHandlerCommandParsingTests()
    {
        var options = TestHelpers.CreateOptions();
        var command = new AddKarmaCommand(_mockKarmaRepo.Object);
        var parser = new SlackParser(_mockMemberRepo.Object);
        _handler = new IncrementMessageKarmaEventHandler(command, parser, _mockMemberRepo.Object, _mockBroker.Object,
            options,
            NullLogger<IncrementMessageKarmaEventHandler>.Instance);

        _mockKarmaRepo.Setup(r => r.AddAsync(It.IsAny<Karma>())).Returns(Task.CompletedTask);
        _mockBroker.Setup(b => b.SendReactionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "someuser", Channel = "C1", Ts = "ts1" };

    [Fact]
    public async Task ParseCommandText_ReasonlessOneWordRecipient()
    {
        _mockMemberRepo.Setup(r => r.GetByUsernameAsync("recipient"))
            .ReturnsAsync(new Member { Username = "recipient" });

        await _handler.HandleAsync(CreateMessage("++ recipient"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "recipient" && k.Reason == "")), Times.Once());
    }

    [Fact]
    public async Task ParseCommandText_ReasonlessOneWordSlackUserRecipient()
    {
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync("SLACK1"))
            .ReturnsAsync(new Member { Username = "slackuser", SlackId = "SLACK1" });
        _mockMemberRepo.Setup(r => r.GetByUsernameAsync("slackuser"))
            .ReturnsAsync(new Member { Username = "slackuser" });

        await _handler.HandleAsync(CreateMessage("++ <@SLACK1>"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "slackuser" && k.Reason == "")), Times.Once());
    }

    [Fact]
    public async Task ParseCommandText_ReasonlessMultiWordRecipient()
    {
        _mockMemberRepo.Setup(r => r.GetByUsernameAsync("some")).ReturnsAsync((Member?)null);

        await _handler.HandleAsync(CreateMessage("++ some recipient"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "some recipient" && k.Reason == "")), Times.Once());
    }

    [Fact]
    public async Task ParseCommandText_ReasonedOneWordRecipient()
    {
        // "++ recipient for some reason" - splits on " for " → recipient="recipient", reason="some reason"
        await _handler.HandleAsync(CreateMessage("++ recipient for some reason"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "recipient" && k.Reason == "some reason")), Times.Once());
    }

    [Fact]
    public async Task ParseCommandText_ReasonedMultiWordRecipient()
    {
        // "++ some recipient for some reason" - splits on " for " → recipient="some recipient", reason="some reason"
        await _handler.HandleAsync(CreateMessage("++ some recipient for some reason"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "some recipient" && k.Reason == "some reason")), Times.Once());
    }

    [Fact]
    public async Task ParseCommandText_ReasonedUserRecipientNoFor()
    {
        // "++ username is sexy" - no " for ", first word "username" is a known user → recipient="username", remaining="is sexy"
        _mockMemberRepo.Setup(r => r.GetByUsernameAsync("username")).ReturnsAsync(new Member { Username = "username" });

        await _handler.HandleAsync(CreateMessage("++ username is sexy"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "username" && k.Reason == "is sexy")), Times.Once());
    }

    [Fact]
    public async Task ParseCommandText_ReasonedUserRecipientWithFor()
    {
        // "++ username for being sexy" - splits on " for " → recipient="username", reason="being sexy"
        await _handler.HandleAsync(CreateMessage("++ username for being sexy"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "username" && k.Reason == "being sexy")), Times.Once());
    }

    [Fact]
    public async Task ParseCommandText_ReasonedSlackIdRecipientWithFor()
    {
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync("SLACK1"))
            .ReturnsAsync(new Member { Username = "slackuser", SlackId = "SLACK1" });

        await _handler.HandleAsync(CreateMessage("++ <@SLACK1> for being sexy"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "slackuser" && k.Reason == "being sexy")), Times.Once());
    }

    [Fact]
    public async Task ParseCommandText_MultipleFor()
    {
        // "++ username for being sexy and for being nice" → reason="being sexy and for being nice"
        await _handler.HandleAsync(CreateMessage("++ username for being sexy and for being nice"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "username" && k.Reason == "being sexy and for being nice")), Times.Once());
    }
}