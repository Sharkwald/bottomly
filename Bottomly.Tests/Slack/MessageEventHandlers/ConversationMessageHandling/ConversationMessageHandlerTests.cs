using Bottomly.LlmBot;
using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet;
using SlackNet.Events;
using SlackNet.WebApi;

namespace Bottomly.Tests.Slack.MessageEventHandlers.ConversationMessageHandling;

public class ConversationMessageHandlerTests
{
    private readonly Mock<ILlmClient> _mockLlmBroker = new();
    private readonly Mock<ISlackMessageBroker> _mockSlackBroker = new();
    private readonly Mock<ISlackApiClient> _mockApiClient = new();
    private readonly Mock<IConversationsApi> _mockConversations = new();
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();
    private readonly Mock<IFeatureFlagRepository> _mockFeatureFlagRepo = new();
    private readonly ConversationMessageHandler _handler;

    public ConversationMessageHandlerTests()
    {
        _mockApiClient.Setup(a => a.Conversations).Returns(_mockConversations.Object);
        _mockFeatureFlagRepo.Setup(r => r.GetAsync("EnableLlm")).ReturnsAsync(true);
        _mockMemberRepo.Setup(r => r.GetByUsernameAsync("bottomly"))
            .ReturnsAsync(new Member { Username = "bottomly", SlackId = "UBOTID" });

        _handler = new ConversationMessageHandler(
            _mockLlmBroker.Object,
            _mockSlackBroker.Object,
            _mockApiClient.Object,
            _mockMemberRepo.Object,
            _mockFeatureFlagRepo.Object,
            NullLogger<ConversationMessageHandler>.Instance);
    }

    private static MessageEvent CreateMessage(string text, string user = "U1", string channel = "C1", string? threadTs = null) =>
        new() { Text = text, User = user, Channel = channel, Ts = "ts1", ThreadTs = threadTs };

    [Theory]
    [InlineData("hey bottomly what do you think?")]
    [InlineData("bottomly, help me")]
    [InlineData("I asked bottomly already")]
    [InlineData("<@UBOTID> what do you think?")]
    [InlineData("<@UBOTID>")]
    public void CanHandle_MessageContainsBottomly_ReturnsTrue(string text) =>
        _handler.CanHandle(CreateMessage(text)).ShouldBeTrue();

    [Theory]
    [InlineData("hello there")]
    [InlineData("_karma alice")]
    [InlineData("")]
    [InlineData("<@UOTHERID> what do you think?")]
    public void CanHandle_MessageWithoutBottomly_ReturnsFalse(string text) =>
        _handler.CanHandle(CreateMessage(text)).ShouldBeFalse();

    [Fact]
    public void BuildHelpMessage_ReturnsEmptyString() =>
        _handler.BuildHelpMessage().ShouldBeEmpty();

    [Fact]
    public async Task HandleAsync_SuccessfulLlmResponse_SendsReplyToChannel()
    {
        SetupConversationHistory("C1", []);
        SetupMembers([new Member { SlackId = "U1", Username = "alice" }]);
        _mockLlmBroker.Setup(b => b.Respond(It.IsAny<BottomlyInputMessage>(), It.IsAny<MessageHistoryContext>()))
            .ReturnsAsync(LlmMessageResponse.Create("Indeed, sir."));

        await _handler.HandleAsync(CreateMessage("bottomly what is 2+2?", "U1", "C1"));

        _mockSlackBroker.Verify(b => b.SendMessageAsync("Indeed, sir.", "C1", null), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_BuildsContextFromHistory()
    {
        var historyMessages = new List<MessageEvent>
        {
            new() { User = "U1", Text = "first message", Ts = "1000.000" },
            new() { User = "U2", Text = "second message", Ts = "1001.000" }
        };
        SetupConversationHistory("C1", historyMessages);
        SetupMembers([
            new Member { SlackId = "U1", Username = "alice" },
            new Member { SlackId = "U2", Username = "bob" }
        ]);
        _mockLlmBroker.Setup(b => b.Respond(It.IsAny<BottomlyInputMessage>(), It.IsAny<MessageHistoryContext>()))
            .ReturnsAsync(LlmMessageResponse.Create("Of course."));

        await _handler.HandleAsync(CreateMessage("bottomly something", "U1", "C1"));

        _mockLlmBroker.Verify(b => b.Respond(It.IsAny<BottomlyInputMessage>(), It.IsAny<MessageHistoryContext>()), Times.Once());
    }

    [Theory]
    [InlineData(nameof(LlmTimeoutResponse))]
    [InlineData(nameof(LlmUsageExceededResponse))]
    [InlineData(nameof(LlmUnknownErrorResponse))]
    public async Task HandleAsync_ErrorLlmResponse_SendsReplyToMessage(string responseType)
    {
        SetupConversationHistory("C1", []);
        SetupMembers([new Member { SlackId = "U1", Username = "alice" }]);
        LlmResponse errorResponse = responseType switch
        {
            nameof(LlmTimeoutResponse) => new LlmTimeoutResponse(),
            nameof(LlmUsageExceededResponse) => new LlmUsageExceededResponse(),
            _ => new LlmUnknownErrorResponse()
        };
        _mockLlmBroker.Setup(b => b.Respond(It.IsAny<BottomlyInputMessage>(), It.IsAny<MessageHistoryContext>()))
            .ReturnsAsync(errorResponse);

        await _handler.HandleAsync(CreateMessage("bottomly what is 2+2?", "U1", "C1"));

        _mockSlackBroker.Verify(b => b.SendMessageAsync(It.IsAny<string>(), "C1", "ts1"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ErrorLlmResponseInThread_SendsReplyToThread()
    {
        SetupConversationHistory("C1", []);
        SetupMembers([new Member { SlackId = "U1", Username = "alice" }]);
        _mockLlmBroker.Setup(b => b.Respond(It.IsAny<BottomlyInputMessage>(), It.IsAny<MessageHistoryContext>()))
            .ReturnsAsync(new LlmTimeoutResponse());

        await _handler.HandleAsync(CreateMessage("bottomly what is 2+2?", "U1", "C1", threadTs: "thread_ts1"));

        _mockSlackBroker.Verify(b => b.SendMessageAsync(It.IsAny<string>(), "C1", "thread_ts1"), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_LlmFlagDisabled_SkipsLlmAndSendsNothing()
    {
        _mockFeatureFlagRepo.Setup(r => r.GetAsync("EnableLlm")).ReturnsAsync(false);

        await _handler.HandleAsync(CreateMessage("bottomly what is 2+2?", "U1", "C1"));

        _mockLlmBroker.Verify(
            b => b.Respond(It.IsAny<BottomlyInputMessage>(), It.IsAny<MessageHistoryContext>()),
            Times.Never());
        _mockSlackBroker.Verify(
            b => b.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
            Times.Never());
    }

    private void SetupConversationHistory(string channel, List<MessageEvent> messages) =>
        _mockConversations
            .Setup(c => c.History(channel, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConversationHistoryResponse { Messages = messages });

    private void SetupMembers(List<Member> members) =>
        _mockMemberRepo.Setup(r => r.GetBySlackIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(members);
}