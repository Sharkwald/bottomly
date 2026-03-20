using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Moq;
using Shouldly;
using SlackNet;
using SlackNet.WebApi;

namespace Bottomly.Tests.Slack;

public class MemberlistPopulatorTests
{
    private readonly Mock<IMemberRepository> _mockRepo = new();
    private readonly Mock<ISlackApiClient> _mockSlack = new();
    private readonly Mock<IUsersApi> _mockUsers = new();
    private readonly MemberlistPopulator _populator;

    public MemberlistPopulatorTests()
    {
        _mockSlack.Setup(s => s.Users).Returns(_mockUsers.Object);
        _populator = new MemberlistPopulator(_mockSlack.Object, _mockRepo.Object);
    }

    [Fact]
    public async Task PopulateMembers_AlreadySeeded_ReturnsEmptyAndSkipsSlack()
    {
        _mockRepo.Setup(r => r.GetByUsernameAsync("owen"))
            .ReturnsAsync(new Member { Username = "owen" });

        var result = await _populator.PopulateMembers();

        result.ShouldBeEmpty();
        _mockUsers.Verify(u => u.List(
            It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task PopulateMembers_NotSeeded_FetchesAndSavesMembers()
    {
        _mockRepo.Setup(r => r.GetByUsernameAsync("owen")).ReturnsAsync((Member?)null);

        var slackUsers = new UserListResponse
        {
            Members =
            [
                new User { Id = "U1", Name = "alice", Deleted = false },
                new User { Id = "U2", Name = "bob", Deleted = false },
                new User { Id = "U3", Name = "deleted_user", Deleted = true }
            ]
        };
        _mockUsers.Setup(u => u.List(
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(slackUsers);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<IEnumerable<Member>>())).Returns(Task.CompletedTask);

        var result = await _populator.PopulateMembers();

        result.Count.ShouldBe(2);
        result.ShouldContain(m => m.Username == "alice" && m.SlackId == "U1");
        result.ShouldContain(m => m.Username == "bob" && m.SlackId == "U2");
        _mockRepo.Verify(r => r.AddAsync(It.Is<IEnumerable<Member>>(members => members.Count() == 2)), Times.Once());
    }
}