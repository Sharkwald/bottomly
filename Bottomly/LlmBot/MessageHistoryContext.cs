namespace Bottomly.LlmBot;

public record MessageHistoryContext
{
    private MessageHistoryContext(List<BottomlyInputMessage> messageHistory, List<BottomlyUserNote> userNotes) =>
        (MessageHistory, UserNotes) = (messageHistory, userNotes);

    public List<BottomlyInputMessage> MessageHistory { get; private init; } = [];
    public List<BottomlyUserNote> UserNotes { get; private init; } = [];

    public static MessageHistoryContext Create(
        List<BottomlyInputMessage> messageHistory,
        List<BottomlyUserNote> userNotes) =>
        new(messageHistory, userNotes);
}