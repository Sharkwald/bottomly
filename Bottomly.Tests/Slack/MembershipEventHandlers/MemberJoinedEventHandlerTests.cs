using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack.MembershipEventHandlers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SlackNet;
using SlackNet.Events;
using SlackNet.WebApi;

namespace Bottomly.Tests.Slack.MembershipEventHandlers;

public class MemberJoinedEventHandlerTests
{
    private readonly MemberJoinedEventHandler _handler;
    private readonly Mock<IMemberRepository> _mockRepo = new();
    private readonly Mock<ISlackApiClient> _mockSlack = new();
    private readonly Mock<IUsersApi> _mockUsers = new();

    public MemberJoinedEventHandlerTests()
    {
        _mockSlack.Setup(s => s.Users).Returns(_mockUsers.Object);
        _handler = new MemberJoinedEventHandler(
            _mockRepo.Object,
            _mockSlack.Object,
            NullLogger<MemberJoinedEventHandler>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_WrongChannel_DoesNothing()
    {
        var ev = new MemberJoinedChannel { Channel = "#random", User = "U1" };

        await _handler.ExecuteAsync(ev);

        _mockUsers.Verify(u => u.Info(It.IsAny<string>()), Times.Never());
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Member>()), Times.Never());
    }

    [Fact]
    public async Task ExecuteAsync_NullMemberInfo_DoesNotSave()
    {
        var ev = new MemberJoinedChannel { Channel = "#general", User = "U1" };
        _mockUsers.Setup(u => u.Info("U1")).ReturnsAsync((User?)null);

        await _handler.ExecuteAsync(ev);

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Member>()), Times.Never());
    }

    [Fact]
    public async Task ExecuteAsync_ValidEvent_AddsNewMember()
    {
        var ev = new MemberJoinedChannel { Channel = "#general", User = "U1" };
        _mockUsers.Setup(u => u.Info("U1")).ReturnsAsync(new User { Id = "U1", Name = "alice" });
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        await _handler.ExecuteAsync(ev);

        _mockRepo.Verify(r => r.AddAsync(It.Is<Member>(m =>
            m.SlackId == "U1" && m.Username == "alice")), Times.Once());
    }
}