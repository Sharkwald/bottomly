using Bottomly.Repositories;

namespace Bottomly.Commands;

public class GetCurrentKarmaReasonsCommand(IKarmaRepository karmaRepository) : ICommand
{
    public string GetPurpose() =>
        "Returns the justifications for someone's/something's current score of imaginary internet points";

    public Task<KarmaReasonsResult> ExecuteAsync(string recipient) =>
        karmaRepository.GetKarmaReasonsAsync(recipient);
}