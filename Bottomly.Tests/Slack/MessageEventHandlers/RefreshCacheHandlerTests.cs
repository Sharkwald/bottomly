using Bottomly.Commands;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.MessageEventHandlers;

public class RefreshCacheHandlerTests
{
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<IMemberRepository> _mockRepo = new();
    private readonly RefreshCacheHandler _handler;

    public RefreshCacheHandlerTests()
    {
        var command = new RefreshCacheCommand(_mockRepo.Object, NullLogger<RefreshCacheCommand>.Instance);
        _handler = new RefreshCacheHandler(
            command,
            _mockBroker.Object,
            TestHelpers.CreateOptions(),
            NullLogger<RefreshCacheHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text) =>
        new() { Text = text, User = "alice", Channel = "C1", Ts = "ts1" };

    // ─── CanHandle ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("_refreshcache")]
    [InlineData("_refreshcache -?")]
    public void CanHandle_RefreshCacheCommand_ReturnsTrue(string text) =>
        _handler.CanHandle(CreateMessage(text)).ShouldBeTrue();

    [Theory]
    [InlineData("_release")]
    [InlineData("_test")]
    [InlineData("refreshcache")]
    [InlineData("")]
    public void CanHandle_OtherCommand_ReturnsFalse(string text) =>
        _handler.CanHandle(CreateMessage(text)).ShouldBeFalse();

    // ─── Success ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_Success_SendsMemberCountMessage()
    {
        _mockRepo.Setup(r => r.InvalidateCacheAsync()).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new() { SlackId = "U1", Username = "alice" },
            new() { SlackId = "U2", Username = "bob" }
        ]);

        await _handler.HandleAsync(CreateMessage("_refreshcache"));

        _mockBroker.Verify(
            b => b.SendMessageAsync(It.Is<string>(s => s.Contains("2")), "C1", It.IsAny<string?>()),
            Times.Once());
    }

    // ─── Error ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_Error_SendsFailureMessage()
    {
        _mockRepo.Setup(r => r.InvalidateCacheAsync()).ThrowsAsync(new Exception("DB unavailable"));

        await _handler.HandleAsync(CreateMessage("_refreshcache"));

        _mockBroker.Verify(
            b => b.SendMessageAsync(It.Is<string>(s => s.Contains("Failed")), "C1", It.IsAny<string?>()),
            Times.Once());
    }
}
