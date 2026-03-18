using System.Text;
using Microsoft.Extensions.AI;

namespace Bottomly.LlmBot;

public record FullPromptContext
{
    private FullPromptContext(ChatMessage historyContext, ChatMessage promptingMessage) =>
        (HistoryContext, PromptingMessage) = (historyContext, promptingMessage);

    public static ChatMessage SystemPrompt =>
        new(ChatRole.System,
            """
            You are a helpful assistant and your name is Bottomly.
            Your character is based on Jeeves from the PG Wodehouse novels. All responses should feature his form of 
            speech; be polite and respectful, with a hint of superiority.
            You are participating in a Slack chat, so responses should be short and to the point, like spoken dialogue.
            """);

    public ChatMessage HistoryContext { get; }
    public ChatMessage PromptingMessage { get; }

    public static FullPromptContext Create(BottomlyInputMessage userPrompt, MessageHistoryContext historyContext)
        => new(
            new ChatMessage(ChatRole.User, historyContext.ToChatContext()),
            new ChatMessage(ChatRole.User, userPrompt.ToChatPromptMessage()));

    public ChatMessage[] ToArray() => [SystemPrompt, HistoryContext, PromptingMessage];
}

public static class LlmBrokerExtensions
{
    public static string ToChatContext(this MessageHistoryContext historyContext) =>
        new StringBuilder()
            .AppendLine("**Begin Prompt Context:**")
            .AppendLine("_Message History:_")
            .AppendLine(string.Join("\n", historyContext.MessageHistory.Select(m => m.ToChatContextMessage())))
            .AppendLine("_User Info:_")
            .AppendLine(string.Join("\n", historyContext.UserNotes.Select(n => $"{n.Username}: {n.Note}")))
            .AppendLine("**End Prompt Context**")
            .ToString();

    extension(BottomlyInputMessage message)
    {
        public string ToChatPromptMessage() =>
            new StringBuilder()
                .AppendLine("**Begin Main Prompt:**")
                .AppendLine($"_User to respond to is {message.Username}_")
                .AppendLine(message.Text)
                .AppendLine("**End Main Prompt**")
                .ToString();

        private string ToChatContextMessage() =>
            $"{message.Username}: {message.Text}";
    }
}