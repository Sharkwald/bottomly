using Bottomly.Models;

namespace Bottomly.LlmBot;

public record BottomlyUserNote
{
    private BottomlyUserNote(string username, string fullName, Gender gender, SassLevel sassLevel, string miscInfo) =>
        (Username, FullName, Gender, SassLevel, MiscInfo) = (username, fullName, gender, sassLevel, miscInfo);

    public string Username { get; private init; } = "";
    public string FullName { get; private init; } = "";
    public Gender Gender { get; private init; } = Gender.Unknown;
    public SassLevel SassLevel { get; private init; } = SassLevel.Moderate;
    public string MiscInfo { get; private init; } = "";

    public static BottomlyUserNote Create(string username, string fullName, Gender gender, SassLevel sassLevel,
        string miscInfo) =>
        new(username, fullName, gender, sassLevel, miscInfo);
}