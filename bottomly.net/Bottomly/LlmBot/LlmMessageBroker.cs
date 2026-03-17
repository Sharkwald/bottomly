using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Bottomly.LlmBot;

public interface ILlmMessageBroker
{
    Task<LlmResponse> Respond(BottomlyInputMessage userPrompt, MessageHistoryContext historyContext);
}

public class LlmMessageBroker(IChatClient chatClient, ILogger<LlmMessageBroker> logger) : ILlmMessageBroker
{
    public async Task<LlmResponse> Respond(BottomlyInputMessage userPrompt, MessageHistoryContext historyContext)
    {
        var options = new ChatOptions
        {
            Temperature = 0.2f
        };

        var fullContext = FullPromptContext.Create(userPrompt, historyContext);

        logger.LogDebug("Sending prompt to LLM: {System}, {Context}, {Prompt}",
            FullPromptContext.SystemPrompt.Text,
            fullContext.HistoryContext.Text,
            fullContext.PromptingMessage.Text);

        try
        {
            return (await chatClient.GetResponseAsync(fullContext.ToArray(), options)).ToSuccessResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get response from LLM");
            return ex.ToErrorResponse();
        }
    }
}

public abstract record LlmResponse;

public record LlmMessageResponse : LlmResponse
{
    private LlmMessageResponse(string message) => Message = message;
    public string Message { get; }

    public static LlmResponse Create(string message) => new LlmMessageResponse(message);
    public static LlmResponse Create(ChatResponse chatResponse) => new LlmMessageResponse(chatResponse.Text);
}

public record LlmTimeoutResponse : LlmResponse;

public record LlmUsageExceededResponse : LlmResponse;

public record LlmUnknownErrorResponse : LlmResponse;

public static class LlmResponseExtensions
{
    public static LlmResponse ToSuccessResponse(this ChatResponse chatResponse) =>
        LlmMessageResponse.Create(chatResponse);

    public static LlmResponse ToErrorResponse(this Exception ex) =>
        ex switch
        {
            TimeoutException => new LlmTimeoutResponse(),
            _ when ex.Message.Contains("usage") => new LlmUsageExceededResponse(),
            _ => new LlmUnknownErrorResponse()
        };

    public static bool IsError(this LlmResponse response) => response is not LlmMessageResponse;
}