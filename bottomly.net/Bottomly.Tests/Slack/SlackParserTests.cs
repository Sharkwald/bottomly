using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Slack;

public class SlackParserTests
{
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();
    private readonly SlackParser _parser;

    public SlackParserTests() => _parser = new SlackParser(_mockMemberRepo.Object);

    [Fact]
    public async Task ReplaceSlackIdTokensWithUsernamesAsync_NoSlackIds_ReturnsUnchanged()
    {
        var message = "hello world";

        var result = await _parser.ReplaceSlackIdTokensWithUsernamesAsync(message);

        result.ShouldBe("hello world");
    }

    [Fact]
    public async Task ReplaceSlackIdTokensWithUsernamesAsync_WithSlackId_ReplacesWithUsername()
    {
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync("U12345"))
            .ReturnsAsync(new Member { Username = "alice", SlackId = "U12345" });

        var result = await _parser.ReplaceSlackIdTokensWithUsernamesAsync("hello <@U12345>");

        result.ShouldBe("hello alice");
    }

    [Fact]
    public async Task ReplaceSlackIdTokensWithUsernamesAsync_SlackIdInLongMessage_ReplacesCorrectly()
    {
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync("U99999"))
            .ReturnsAsync(new Member { Username = "bob", SlackId = "U99999" });

        var result = await _parser.ReplaceSlackIdTokensWithUsernamesAsync("good work <@U99999> you did it");

        result.ShouldBe("good work bob you did it");
    }
}