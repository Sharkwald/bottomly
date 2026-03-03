using Bottomly.Commands;
using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.ReactionHandlers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.ReactionHandlers;

public class AddKarmaReactionHandlerTests
{
    private readonly AddKarmaReactionHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<IKarmaRepository> _mockKarmaRepo = new();
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();

    public AddKarmaReactionHandlerTests()
    {
        var command = new AddKarmaCommand(_mockKarmaRepo.Object);
        _handler = new AddKarmaReactionHandler(
            command,
            new KarmaReactionMap(),
            _mockMemberRepo.Object,
            _mockBroker.Object,
            NullLogger<AddKarmaReactionHandler>.Instance);
    }

    [Fact]
    public void ParseReaction_WithSkinTone_StripsSkinTone()
    {
        // Test via CanHandle: "+1::skin-tone-2" should resolve to "+1" which is in karma reactions
        var ev = new ReactionAdded { Reaction = "+1::skin-tone-2" };
        _handler.CanHandle(ev).ShouldBeTrue();
    }

    [Fact]
    public void CanHandle_ValidReaction_ReturnsTrue()
    {
        var ev = new ReactionAdded { Reaction = "joy" };
        _handler.CanHandle(ev).ShouldBeTrue();
    }

    [Fact]
    public void CanHandle_ValidReactionWithSkinTone_ReturnsTrue()
    {
        var ev = new ReactionAdded { Reaction = "+1::skin-tone-2" };
        _handler.CanHandle(ev).ShouldBeTrue();
    }

    [Fact]
    public void CanHandle_InvalidReaction_ReturnsFalse()
    {
        var ev = new ReactionAdded { Reaction = "robot_face" };
        _handler.CanHandle(ev).ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_ExecutesCommandAndSendsReaction()
    {
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync("U_reactor"))
            .ReturnsAsync(new Member { Username = "reactor", SlackId = "U_reactor" });
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync("U_reactee"))
            .ReturnsAsync(new Member { Username = "reactee", SlackId = "U_reactee" });
        _mockKarmaRepo.Setup(r => r.AddAsync(It.IsAny<Karma>())).Returns(Task.CompletedTask);
        _mockBroker.Setup(b => b.SendReactionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var ev = new ReactionAdded
        {
            Reaction = "joy",
            User = "U_reactor",
            ItemUser = "U_reactee",
            Item = new ReactionMessage { Channel = "C1", Ts = "ts1" }
        };

        await _handler.HandleAsync(ev);

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "reactee" &&
            k.AwardedByUsername == "reactor" &&
            k.KarmaType == KarmaType.PozzyPoz)), Times.Once());
        _mockBroker.Verify(b => b.SendReactionAsync("robot_face", "C1", "ts1"), Times.Once());
    }
}