using Bottomly.Models;
using Bottomly.Repositories;

namespace Bottomly.Commands;

public class AddKarmaCommand(IKarmaRepository karmaRepository) : ICommand
{
    public string GetPurpose() => "Awards an imaginary internet point to someone/something.";

    public async Task<Karma> ExecuteAsync(string awardedTo, string awardedBy, string reason, KarmaType karmaType)
    {
        if (karmaType == KarmaType.PozzyPoz && awardedBy.Equals(awardedTo, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Can't give yourself positive karma");
        }

        var karma = new Karma
        {
            AwardedToUsername = awardedTo,
            AwardedByUsername = awardedBy,
            Reason = reason,
            Awarded = DateTime.UtcNow,
            KarmaType = karmaType
        };

        await karmaRepository.AddAsync(karma);
        return karma;
    }
}