using Bottomly.LlmBot;
using Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;
using Microsoft.Extensions.AI;
using Shouldly;

namespace Bottomly.Tests.Slack.MessageEventHandlers.ConversationMessageHandling;

public class ResponseMessageFactoryTests
{
    [Fact]
    public void ToSlackResponse_LlmMessageResponse_ReturnsMessage()
    {
        var chatResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, "Hello, sir.")]);
        var response = chatResponse.ToSuccessResponse();

        var result = response.ToSlackResponse();

        result.ShouldBe("Hello, sir.");
    }

    [Fact]
    public void ToSlackResponse_LlmTimeoutResponse_ReturnsTimeoutMessage()
    {
        LlmResponse response = new LlmTimeoutResponse();

        var result = response.ToSlackResponse();

        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ToSlackResponse_LlmUsageExceededResponse_ReturnsUsageMessage()
    {
        LlmResponse response = new LlmUsageExceededResponse();

        var result = response.ToSlackResponse();

        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ToSlackResponse_LlmUnknownErrorResponse_ReturnsFallbackMessage()
    {
        LlmResponse response = new LlmUnknownErrorResponse();

        var result = response.ToSlackResponse();

        result.ShouldNotBeNullOrEmpty();
    }
}
