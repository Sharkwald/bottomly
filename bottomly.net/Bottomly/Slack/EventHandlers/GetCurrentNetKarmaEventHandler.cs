using System.Text;
using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class GetCurrentNetKarmaEventHandler(
    GetCurrentNetKarmaCommand command,
    SlackParser parser,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GetCurrentNetKarmaEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Get Current Karma";
    public override ICommand Command => command;
    protected override string CommandSymbol => "karma";
    public override string GetUsage() => CommandTrigger + "[recipient <if blank, will default to you>]";

    public override string GetUsageAddendum()
    {
        var reactions = AddKarmaCommand.GetKarmaReactions();
        var lines = new StringBuilder($"{Environment.NewLine}Giving Karma with reactions:{Environment.NewLine}");
        foreach (var kvp in reactions)
        {
            lines.AppendLine($":{kvp.Key}: will {(kvp.Value == KarmaType.PozzyPoz ? "PozzyPoz" : "NeggyNeg")}");
        }

        return lines.ToString();
    }

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
        await SendMessageResponseAsync($"{recipient}: {result}", message);
    }
}