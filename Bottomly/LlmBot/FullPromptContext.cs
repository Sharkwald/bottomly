using System.Text.Json;
using System.Text.Json.Serialization;
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
            new ChatMessage(ChatRole.System, historyContext.ToChatContext()),
            new ChatMessage(ChatRole.User, userPrompt.ToChatPromptMessage()));

    public ChatMessage[] ToArray() => [SystemPrompt, HistoryContext, PromptingMessage];
}

public static class LlmClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static string ToChatContext(this MessageHistoryContext historyContext)
    {
        var payload = new PromptContextPayload(
            historyContext.MessageHistory.Select(m => new MessageHistoryEntry(m.Username, m.Text)).ToList(),
            historyContext.UserNotes.Select(n => new UserInfoEntry(n.Username, n.FullName, n.Gender.ToString(),
                n.SassLevel.ToString(), n.MiscInfo)).ToList()
        );
        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    extension(BottomlyInputMessage message)
    {
        public string ToChatPromptMessage() => $"{message.Username}: {message.Text}";
    }

    private record PromptContextPayload(
        [property: JsonPropertyName("message_history")]
        List<MessageHistoryEntry> MessageHistory,
        [property: JsonPropertyName("users")] List<UserInfoEntry> Users);

    private record MessageHistoryEntry(
        [property: JsonPropertyName("username")]
        string Username,
        [property: JsonPropertyName("text")] string Text);

    private record UserInfoEntry(
        [property: JsonPropertyName("username")]
        string Username,
        [property: JsonPropertyName("full_name")]
        string FullName,
        [property: JsonPropertyName("gender")] string Gender,
        [property: JsonPropertyName("sass_level")]
        string SassLevel,
        [property: JsonPropertyName("misc_info")]
        string MiscInfo);
}