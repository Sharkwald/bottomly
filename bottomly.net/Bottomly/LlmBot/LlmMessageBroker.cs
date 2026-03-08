using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Bottomly.LlmBot;

public class LlmMessageBroker(IChatClient chatClient, ILogger<LlmMessageBroker> logger)
{
    public async Task<ChatResponse> Respond(BottomlyInputMessage prompt, ChatMessageContext context)
    {
        var systemMessage = new ChatMessage(ChatRole.System,
            """
            You are a helpful assistant and your name is Bottomly.
            Your character is based on Jeeves from the PG Wodehouse novels. All responses should feature his form of 
            speech; be polite and respectful, with a hint of superiority.
            You are participating in a Slack chat, so responses should be short and to the point, like spoken dialogue.
            """);

        var promptContext = new ChatMessage(ChatRole.User, context.ToChatContext());
        var promptMessage = new ChatMessage(ChatRole.User, prompt.ToChatInput());
        var options = new ChatOptions
        {
            Temperature = 0
        };

        logger.LogDebug("Sending prompt to LLM: {Context}, {Prompt}", promptContext.Text, promptMessage.Text);

        return await chatClient.GetResponseAsync([systemMessage, promptContext, promptMessage], options);
    }
}

public record BottomlyInputMessage
{
    public string Username { get; private init; } = "";
    public string Text { get; private init; } = "";

    public static BottomlyInputMessage Create(string username, string text) =>
        new() { Username = username, Text = text };
}

public record BottomlyUserNote
{
    public string Username { get; private init; } = "";
    public string Note { get; private init; } = "";

    public static BottomlyUserNote Create(string username, string note) => new() { Username = username, Note = note };
}

public record ChatMessageContext
{
    public List<BottomlyInputMessage> MessageHistory { get; private init; } = [];
    public List<BottomlyUserNote> UserNotes { get; private init; } = [];

    public static ChatMessageContext Create(List<BottomlyInputMessage> messageHistory,
        List<BottomlyUserNote> userNotes) =>
        new() { MessageHistory = messageHistory, UserNotes = userNotes };
}

public static class LlmBrokerExtensions
{
    public static string ToChatInput(this BottomlyInputMessage message) => $"{message.Username}: {message.Text}";

    public static string ToChatContext(this ChatMessageContext context) =>
        new StringBuilder()
            .AppendLine("**Context:**")
            .AppendLine("_Message History:_")
            .AppendLine(string.Join("\n", context.MessageHistory.Select(m => m.ToChatInput())))
            .AppendLine("_User Info:_")
            .AppendLine(string.Join("\n", context.UserNotes.Select(n => $"{n.Username}: {n.Note}")))
            .ToString();
}