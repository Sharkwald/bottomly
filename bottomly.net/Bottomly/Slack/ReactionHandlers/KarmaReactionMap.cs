using System.Text;
using Bottomly.Models;

namespace Bottomly.Slack.ReactionHandlers;

public class KarmaReactionMap
{
    private Dictionary<string, KarmaType> Map { get; } = new()
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


    public bool IsKarmaReaction(string reaction) => Map.ContainsKey(reaction);

    public KarmaType GetKarmaType(string reaction) => Map[reaction];

    public string KarmaReactionDescriptions()
    {
        var lines = new StringBuilder($"{Environment.NewLine}Giving Karma with reactions:{Environment.NewLine}");

        foreach (var (key, value) in Map)
        {
            lines.AppendLine($":{key}: will {value}");
        }

        return lines.ToString();
    }
}