using System.Text;
using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class HelpHandler(
    IEnumerable<IMessageEventHandler> handlers,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<HelpHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    private static readonly string[] HelpSymbols = ["help", "?", "list"];

    public override string Name => "Help";
    public override ICommand? Command => null;
    protected override string CommandSymbol => HelpSymbols[0];

    protected override string GetUsage()
    {
        var parts = HelpSymbols.Select(s => $"`{Prefix}{s}`");
        return string.Join(" or ", parts);
    }

    public override bool CanHandle(MessageEvent message)
    {
        var first = message.Text?.Split(' ')[0];
        return HelpSymbols.Any(s => first == Prefix + s);
    }

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var sb = new StringBuilder();
        foreach (var handler in handlers)
        {
            sb.AppendLine(handler.BuildHelpMessage());
        }

        await SendDmResponseAsync(sb.ToString().Trim(), message);
    }
}