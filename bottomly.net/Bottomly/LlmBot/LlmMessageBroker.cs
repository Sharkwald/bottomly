using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Bottomly.LlmBot;

public class LlmMessageBroker(IChatClient chatClient, ILogger<LlmMessageBroker> logger)
{
    public async Task<ChatResponse> Respond(BottomlyInputMessage userPrompt, MessageHistoryContext historyContext)
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

        return await chatClient.GetResponseAsync(fullContext.ToArray(), options);
    }
}