using Bottomly.LlmBot;
using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet;
using SlackNet.Events;
using SlackNet.WebApi;

namespace Bottomly.Tests.Slack.MessageEventHandlers.ConversationMessageHandling;

public class ConversationMessageHandlerTests
{
    private readonly Mock<IChatClient> _mockChatClient = new();
    private readonly Mock<ISlackMessageBroker> _mockSlackBroker = new();
    private readonly Mock<ISlackApiClient> _mockApiClient = new();
    private readonly Mock<IConversationsApi> _mockConversations = new();
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();
    private readonly ConversationMessageHandler _handler;

    public ConversationMessageHandlerTests()
    {
        _mockApiClient.Setup(a => a.Conversations).Returns(_mockConversations.Object);

        var llmBroker = new LlmMessageBroker(_mockChatClient.Object, NullLogger<LlmMessageBroker>.Instance);
        _handler = new ConversationMessageHandler(
            llmBroker,
            _mockSlackBroker.Object,
            _mockApiClient.Object,
            _mockMemberRepo.Object);
    }

    private static MessageEvent CreateMessage(string text, string user = "U1", string channel = "C1") =>
        new() { Text = text, User = user, Channel = channel, Ts = "ts1" };

    [Theory]
    [InlineData("hey bottomly what do you think?")]
    [InlineData("bottomly, help me")]
    [InlineData("I asked bottomly already")]
    public void CanHandle_MessageContainsBottomly_ReturnsTrue(string text) =>
        _handler.CanHandle(CreateMessage(text)).ShouldBeTrue();

    [Theory]
    [InlineData("hello there")]
    [InlineData("_karma alice")]
    [InlineData("")]
    public void CanHandle_MessageWithoutBottomly_ReturnsFalse(string text) =>
        _handler.CanHandle(CreateMessage(text)).ShouldBeFalse();

    [Fact]
    public void BuildHelpMessage_ReturnsEmptyString() =>
        _handler.BuildHelpMessage().ShouldBeEmpty();

    [Fact]
    public async Task HandleAsync_SuccessfulLlmResponse_SendsReplyToChannel()
    {
        SetupConversationHistory("C1", []);
        _mockMemberRepo.Setup(r => r.GetBySlackIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([new Member { SlackId = "U1", Username = "alice" }]);

        var chatResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, "Indeed, sir.")]);
        _mockChatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatResponse);

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
        _mockMemberRepo.Setup(r => r.GetBySlackIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Member { SlackId = "U1", Username = "alice" },
                new Member { SlackId = "U2", Username = "bob" }
            ]);

        var chatResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, "Of course.")]);
        _mockChatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatResponse);

        await _handler.HandleAsync(CreateMessage("bottomly something", "U1", "C1"));

        _mockChatClient.Verify(c =>
            c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()), Times.Once());
    }

    private void SetupConversationHistory(string channel, List<MessageEvent> messages)
    {
        _mockConversations
            .Setup(c => c.History(channel, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConversationHistoryResponse { Messages = messages });
    }
}
