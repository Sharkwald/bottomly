using Bottomly.Repositories;

namespace Bottomly.Commands;

public class GetLoserBoardCommand(IKarmaRepository karmaRepository) : ICommand
{
    public string GetPurpose() => "Shows the worst of the worst!";

    public Task<IReadOnlyList<KarmaScore>> ExecuteAsync(int size = 3) =>
        karmaRepository.GetLoserBoardAsync(size);
}