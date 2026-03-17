namespace Bottomly.LlmBot;

public record BottomlyInputMessage
{
    public string Username { get; private init; } = "";
    public string Text { get; private init; } = "";

    public static BottomlyInputMessage Create(string username, string text) =>
        new() { Username = username, Text = text };
}