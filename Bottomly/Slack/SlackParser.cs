using System.Text.RegularExpressions;
using Bottomly.Repositories;

namespace Bottomly.Slack;

public partial class SlackParser(IMemberRepository memberRepository)
{
    private static readonly Regex SlackIdPattern = SlackIdRegex();

    public async Task<string> ReplaceSlackIdTokensWithUsernamesAsync(string message)
    {
        var matches = SlackIdPattern.Matches(message);
        if (matches.Count == 0)
        {
            return message;
        }

        foreach (Match match in matches)
        {
            var slackId = match.Groups[1].Value;
            var member = await memberRepository.GetBySlackIdAsync(slackId);
            if (member is not null)
            {
                message = message.Replace(match.Value, member.Username);
            }
        }

        return message;
    }

    [GeneratedRegex(@"<@([A-Z0-9]+)>", RegexOptions.Compiled)]
    private static partial Regex SlackIdRegex();
}