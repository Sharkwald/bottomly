using System.Text.RegularExpressions;
using Bottomly.Repositories;

namespace Bottomly.Slack;

public class SlackParser(IMemberRepository memberRepository)
{
    private static readonly Regex SlackIdPattern = new(@"<@([A-Z0-9]+)>", RegexOptions.Compiled);

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
}