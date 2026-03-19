using Bottomly.Configuration;
using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.MessageEventHandlers;

public class LlmToggleHandlerTests
{
    private readonly Mock<IFeatureFlagRepository> _mockFlagRepo = new();
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly LlmToggleHandler _handler;

    public LlmToggleHandlerTests()
    {
        _handler = new LlmToggleHandler(
            _mockFlagRepo.Object,
            _mockMemberRepo.Object,
            _mockBroker.Object,
            TestHelpers.CreateOptions(),
            NullLogger<LlmToggleHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text, string user = "U_OWEN") =>
        new() { Text = text, User = user, Channel = "C1", Ts = "ts1" };

    private void SetupMemberForSlackId(string slackId, string username) =>
        _mockMemberRepo
            .Setup(r => r.GetBySlackIdAsync(slackId))
            .ReturnsAsync(new Member { SlackId = slackId, Username = username });

    // ─── CanHandle ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("_llm on")]
    [InlineData("_llm off")]
    [InlineData("_llm")]
    [InlineData("_llm unknown")]
    public void CanHandle_LlmCommand_ReturnsTrue(string text) =>
        _handler.CanHandle(CreateMessage(text)).ShouldBeTrue();

    [Theory]
    [InlineData("_karma alice")]
    [InlineData("bottomly what's up?")]
    [InlineData("_search something")]
    [InlineData("")]
    public void CanHandle_NonLlmCommand_ReturnsFalse(string text) =>
        _handler.CanHandle(CreateMessage(text)).ShouldBeFalse();

    // ─── Permission check ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_NonOwenUser_RepliesWithPermissionDenied()
    {
        SetupMemberForSlackId("U_OTHER", "alice");

        await _handler.HandleAsync(CreateMessage("_llm on", "U_OTHER"));

        _mockBroker.Verify(
            b => b.SendMessageAsync(It.Is<string>(s => s.Contains("permission")), "C1", It.IsAny<string?>()),
            Times.Once());
        _mockFlagRepo.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
    }

    [Fact]
    public async Task HandleAsync_UnknownUser_RepliesWithPermissionDenied()
    {
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync("U_UNKNOWN")).ReturnsAsync((Member?)null);

        await _handler.HandleAsync(CreateMessage("_llm on", "U_UNKNOWN"));

        _mockBroker.Verify(
            b => b.SendMessageAsync(It.Is<string>(s => s.Contains("permission")), "C1", It.IsAny<string?>()),
            Times.Once());
        _mockFlagRepo.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
    }

    // ─── !llm on ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_LlmOn_OwenUser_EnablesFlag()
    {
        SetupMemberForSlackId("U_OWEN", "owen");

        await _handler.HandleAsync(CreateMessage("_llm on", "U_OWEN"));

        _mockFlagRepo.Verify(r => r.SetAsync("EnableLlm", true), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_LlmOn_OwenUser_ConfirmsEnabled()
    {
        SetupMemberForSlackId("U_OWEN", "owen");

        await _handler.HandleAsync(CreateMessage("_llm on", "U_OWEN"));

        _mockBroker.Verify(
            b => b.SendMessageAsync(It.Is<string>(s => s.Contains("enabled", StringComparison.OrdinalIgnoreCase)), "C1", It.IsAny<string?>()),
            Times.Once());
    }

    // ─── !llm off ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_LlmOff_OwenUser_DisablesFlag()
    {
        SetupMemberForSlackId("U_OWEN", "owen");

        await _handler.HandleAsync(CreateMessage("_llm off", "U_OWEN"));

        _mockFlagRepo.Verify(r => r.SetAsync("EnableLlm", false), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_LlmOff_OwenUser_ConfirmsDisabled()
    {
        SetupMemberForSlackId("U_OWEN", "owen");

        await _handler.HandleAsync(CreateMessage("_llm off", "U_OWEN"));

        _mockBroker.Verify(
            b => b.SendMessageAsync(It.Is<string>(s => s.Contains("disabled", StringComparison.OrdinalIgnoreCase)), "C1", It.IsAny<string?>()),
            Times.Once());
    }

    // ─── Unknown arg ───────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UnknownArg_OwenUser_RepliesWithUsageError()
    {
        SetupMemberForSlackId("U_OWEN", "owen");

        await _handler.HandleAsync(CreateMessage("_llm blah", "U_OWEN"));

        _mockBroker.Verify(
            b => b.SendMessageAsync(It.Is<string>(s => s.Contains("blah")), "C1", It.IsAny<string?>()),
            Times.Once());
        _mockFlagRepo.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
    }
}
