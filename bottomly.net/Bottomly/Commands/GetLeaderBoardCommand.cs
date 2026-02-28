using Bottomly.Repositories;

namespace Bottomly.Commands;

public class GetLeaderBoardCommand(IKarmaRepository karmaRepository) : ICommand
{
    public string GetPurpose() => "Shows the best of the best!";

    public Task<IReadOnlyList<KarmaScore>> ExecuteAsync(int size = 3) =>
        karmaRepository.GetLeaderBoardAsync(size);
}