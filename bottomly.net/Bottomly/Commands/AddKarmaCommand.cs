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

    public static Dictionary<string, KarmaType> GetKarmaReactions() => new()
    {
        ["+1"] = KarmaType.PozzyPoz,
        ["arrow_up"] = KarmaType.PozzyPoz,
        ["clap"] = KarmaType.PozzyPoz,
        ["heart"] = KarmaType.PozzyPoz,
        ["heart_eyes"] = KarmaType.PozzyPoz,
        ["heavy_plus_sign"] = KarmaType.PozzyPoz,
        ["heavy_tick"] = KarmaType.PozzyPoz,
        ["joy"] = KarmaType.PozzyPoz,
        ["party_parrot"] = KarmaType.PozzyPoz,
        ["raised_hands"] = KarmaType.PozzyPoz,
        ["smile"] = KarmaType.PozzyPoz,
        ["thumbsup"] = KarmaType.PozzyPoz,
        ["-1"] = KarmaType.NeggyNeg,
        ["arrow_down"] = KarmaType.NeggyNeg,
        ["hankey"] = KarmaType.NeggyNeg,
        ["heavy_minus_sign"] = KarmaType.NeggyNeg,
        ["poo"] = KarmaType.NeggyNeg,
        ["poop"] = KarmaType.NeggyNeg,
        ["shit"] = KarmaType.NeggyNeg,
        ["thumbsdown"] = KarmaType.NeggyNeg
    };
}