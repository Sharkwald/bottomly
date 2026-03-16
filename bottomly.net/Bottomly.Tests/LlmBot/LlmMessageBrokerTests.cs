using Bottomly.LlmBot;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Bottomly.Tests.LlmBot;

public class LlmMessageBrokerTests
{
    private readonly Mock<IChatClient> _mockChatClient = new();
    private readonly LlmMessageBroker _broker;

    public LlmMessageBrokerTests()
    {
        _broker = new LlmMessageBroker(_mockChatClient.Object, NullLogger<LlmMessageBroker>.Instance);
    }

    [Fact]
    public async Task Respond_SuccessfulResponse_ReturnsLlmMessageResponse()
    {
        var chatResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, "Quite right, sir.")]);
        _mockChatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatResponse);

        var prompt = BottomlyInputMessage.Create("alice", "What's the weather?");
        var context = MessageHistoryContext.Create([], []);

        var result = await _broker.Respond(prompt, context);

        result.ShouldBeOfType<LlmMessageResponse>();
        ((LlmMessageResponse)result).Message.ShouldBe("Quite right, sir.");
    }

    [Fact]
    public async Task Respond_TimeoutException_ReturnsLlmTimeoutResponse()
    {
        _mockChatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Request timed out"));

        var prompt = BottomlyInputMessage.Create("alice", "Hello?");
        var context = MessageHistoryContext.Create([], []);

        var result = await _broker.Respond(prompt, context);

        result.ShouldBeOfType<LlmTimeoutResponse>();
    }

    [Fact]
    public async Task Respond_UsageExceededException_ReturnsLlmUsageExceededResponse()
    {
        _mockChatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("usage limit exceeded"));

        var prompt = BottomlyInputMessage.Create("alice", "Hello?");
        var context = MessageHistoryContext.Create([], []);

        var result = await _broker.Respond(prompt, context);

        result.ShouldBeOfType<LlmUsageExceededResponse>();
    }

    [Fact]
    public async Task Respond_UnknownException_ReturnsLlmUnknownErrorResponse()
    {
        _mockChatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("something unexpected"));

        var prompt = BottomlyInputMessage.Create("alice", "Hello?");
        var context = MessageHistoryContext.Create([], []);

        var result = await _broker.Respond(prompt, context);

        result.ShouldBeOfType<LlmUnknownErrorResponse>();
    }

    [Fact]
    public async Task Respond_PassesPromptToClient()
    {
        var chatResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, "Indeed.")]);
        _mockChatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatResponse);

        var prompt = BottomlyInputMessage.Create("alice", "test message");
        var context = MessageHistoryContext.Create([], []);

        await _broker.Respond(prompt, context);

        _mockChatClient.Verify(c =>
            c.GetResponseAsync(
                It.Is<IEnumerable<ChatMessage>>(msgs => msgs.Count() == 3),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()), Times.Once());
    }
}
