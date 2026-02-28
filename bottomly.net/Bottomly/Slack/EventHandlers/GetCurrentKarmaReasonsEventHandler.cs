using System.Text;
using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class GetCurrentKarmaReasonsEventHandler(
    GetCurrentKarmaReasonsCommand command,
    SlackParser parser,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GetCurrentKarmaReasonsEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Karma Reasons";
    public override ICommand Command => command;
    protected override string CommandSymbol => "reasons";
    public override string GetUsage() => CommandTrigger + "[recipient <if blank, will default to you>]";

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandTrigger.TrimEnd()) == true;

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var text = await parser.ReplaceSlackIdTokensWithUsernamesAsync(message.Text!);
        var recipient = text[CommandTrigger.Length..].Split(' ')[0];
        if (string.IsNullOrEmpty(recipient))
        {
            recipient = message.User;
        }

        var result = await command.ExecuteAsync(recipient);
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