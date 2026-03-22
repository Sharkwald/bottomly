using Bottomly.Commands;
using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class RefreshCacheCommandTests
{
    private readonly Mock<IMemberRepository> _mockRepo = new();
    private readonly RefreshCacheCommand _command;

    public RefreshCacheCommandTests() =>
        _command = new RefreshCacheCommand(_mockRepo.Object, NullLogger<RefreshCacheCommand>.Instance);

    [Fact]
    public async Task ExecuteAsync_InvalidatesCacheBeforeRehydrating()
    {
        var callOrder = new List<string>();
        _mockRepo.Setup(r => r.InvalidateCacheAsync())
            .Callback(() => callOrder.Add("invalidate"))
            .Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.GetAllAsync())
            .Callback(() => callOrder.Add("getAll"))
            .ReturnsAsync([]);

        await _command.ExecuteAsync();

        callOrder.ShouldBe(["invalidate", "getAll"]);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccessWithMemberCount()
    {
        var members = new List<Member>
        {
            new() { SlackId = "U1", Username = "alice" },
            new() { SlackId = "U2", Username = "bob" }
        };
        _mockRepo.Setup(r => r.InvalidateCacheAsync()).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(members);

        var result = await _command.ExecuteAsync();

        var success = result.ShouldBeOfType<RefreshCacheSuccessResult>();
        success.MemberCount.ShouldBe(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionThrown_ReturnsErrorResult()
    {
        _mockRepo.Setup(r => r.InvalidateCacheAsync()).ThrowsAsync(new Exception("DB error"));

        var result = await _command.ExecuteAsync();

        var error = result.ShouldBeOfType<RefreshCacheErrorResult>();
        error.Error.ShouldBe("DB error");
    }

    [Fact]
    public void GetPurpose_ReturnsNonEmptyDescription()
    {
        _command.GetPurpose().ShouldNotBeNullOrWhiteSpace();
    }
}
