using System.Text;
using Bottomly.Configuration;
using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class GetCurrentKarmaReasonsHandler(
    IKarmaRepository repository,
    SlackParser parser,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GetCurrentKarmaReasonsHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Karma Reasons";
    protected override string CommandSymbol => "reasons";

    protected override string GetPurpose() =>
        "Returns the justifications for someone's/something's current score of imaginary internet points";

    protected override string GetUsage() => CommandTrigger + "[recipient <if blank, will default to you>]";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var text = await parser.ReplaceSlackIdTokensWithUsernamesAsync(message.Text!);
        var recipient = text[CommandTrigger.Length..].Split(' ')[0];
        if (string.IsNullOrEmpty(recipient))
        {
            recipient = message.User;
        }

        var result = await repository.GetKarmaReasonsAsync(recipient);
        var response = BuildResponse(result, recipient);
        await SendDmResponseAsync(response, message);
    }

    private static string BuildResponse(KarmaReasonsResult result, string recipient)
    {
        var sb = new StringBuilder($"Recent Karma for {recipient}:{Environment.NewLine}");
        sb.Append($"Recently awarded with no reason: {result.Reasonless}.");

        if (!result.Reasoned.Any())
        {
            sb.AppendLine();
            sb.Append("None awarded with a reason given.");
            return sb.ToString();
        }

        foreach (var k in result.Reasoned)
        {
            sb.AppendLine();
            var symbol = k.KarmaType == KarmaType.PozzyPoz ? "++" : "--";
            sb.Append($"{symbol} from {k.AwardedByUsername} for \"{k.Reason}\"");
        }

        return sb.ToString();
    }
}