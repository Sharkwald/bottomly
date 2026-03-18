namespace Bottomly.LlmBot;

public record BottomlyUserNote
{
    private BottomlyUserNote(string username, string note) => (Username, Note) = (username, note);
    public string Username { get; private init; } = "";
    public string Note { get; private init; } = "";

    public static BottomlyUserNote Create(string username, string note) => new(username, note);
}