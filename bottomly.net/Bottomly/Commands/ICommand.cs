namespace Bottomly.Commands;

public interface ICommand
{
    static readonly ICommand None = new VoidCommand();
    
    string GetPurpose();
}

public class VoidCommand : ICommand
{
    public string GetPurpose() => "Does nothing.";
}