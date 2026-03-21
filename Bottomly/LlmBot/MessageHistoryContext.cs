namespace Bottomly.LlmBot;

public record MessageHistoryContext
{
    private MessageHistoryContext(IList<BottomlyInputMessage> messageHistory, IList<BottomlyUserNote> userNotes) =>
        (MessageHistory, UserNotes) = (messageHistory, userNotes);

    public IList<BottomlyInputMessage> MessageHistory { get; private init; } = [];
    public IList<BottomlyUserNote> UserNotes { get; private init; } = [];

    public static MessageHistoryContext Create(
        IList<BottomlyInputMessage> messageHistory,
        IList<BottomlyUserNote> userNotes) =>
        new(messageHistory, userNotes);
}