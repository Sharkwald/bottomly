namespace Bottomly.Commands;

public class NoneCommand : ICommand
{
    public static ICommand None { get; } = new NoneCommand();
    public string GetPurpose() => "Does nothing.";
}