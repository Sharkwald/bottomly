using Bottomly.Repositories;

namespace Bottomly.Commands;

public class GetCurrentNetKarmaCommand(IKarmaRepository karmaRepository) : ICommand
{
    public string GetPurpose() => "Returns someone's/something's current score of imaginary internet points";

    public Task<int> ExecuteAsync(string recipient) =>
        karmaRepository.GetCurrentNetKarmaAsync(recipient);
}