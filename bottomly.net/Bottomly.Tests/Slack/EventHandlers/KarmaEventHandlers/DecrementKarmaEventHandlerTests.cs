using Bottomly.Commands;
using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.EventHandlers.KarmaEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.EventHandlers.KarmaEventHandlers;

public class DecrementKarmaEventHandlerTests
{
    private readonly DecrementKarmaEventHandler _handler;
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<IKarmaRepository> _mockKarmaRepo = new();
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();

    public DecrementKarmaEventHandlerTests()
    {
        var options = TestHelpers.CreateOptions();
        var command = new AddKarmaCommand(_mockKarmaRepo.Object);
        var parser = new SlackParser(_mockMemberRepo.Object);
        _handler = new DecrementKarmaEventHandler(command, parser, _mockMemberRepo.Object, _mockBroker.Object, options,
            NullLogger<DecrementKarmaEventHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text, string user = "U_sender") =>
        new() { Text = text, User = user, Channel = "C1", Ts = "ts1" };

    [Fact]
    public void CanHandle_ValidEvent_ReturnsTrue() => _handler.CanHandle(CreateMessage("-- recipient")).ShouldBeTrue();

    [Fact]
    public void CanHandle_InvalidEvent_ReturnsFalse() => _handler.CanHandle(CreateMessage("no prefix")).ShouldBeFalse();

    [Fact]
    public async Task HandleAsync_ValidEvent_ExecutesCommandAndSendsReaction()
    {
        _mockKarmaRepo.Setup(r => r.AddAsync(It.IsAny<Karma>())).Returns(Task.CompletedTask);
        _mockMemberRepo.Setup(r => r.GetByUsernameAsync("recipient"))
            .ReturnsAsync(new Member { Username = "recipient" });
        _mockBroker.Setup(b => b.SendReactionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(CreateMessage("-- recipient"));

        _mockKarmaRepo.Verify(r => r.AddAsync(It.Is<Karma>(k =>
            k.AwardedToUsername == "recipient" &&
            k.KarmaType == KarmaType.NeggyNeg)), Times.Once());
        _mockBroker.Verify(b => b.SendReactionAsync("robot_face", "C1", "ts1"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_HelpEvent_SendsHelpMessage()
    {
        // Help event for karma handlers is "-- -?" (no prefix)
        await _handler.HandleAsync(CreateMessage("-- -?"));

        _mockBroker.Verify(b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Neggy-neg")), "C1", null),
            Times.Once());
    }
}