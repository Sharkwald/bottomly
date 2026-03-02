using System.Text;
using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class GetLoserBoardEventHandler(
    GetLoserBoardCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GetLoserBoardEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Get Loserboard";
    public override ICommand Command => command;
    protected override string CommandSymbol => "loserboard";
    protected override string GetUsage() => CommandTrigger + "[size of loserboard. Default is 3]";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var sizeArg = message.Text![CommandTrigger.Length..];
        var size = int.TryParse(sizeArg, out var parsed) && parsed > 0 ? parsed : 3;

        var result = await command.ExecuteAsync(size);
        var sb = new StringBuilder();
        foreach (var entry in result)
        {
            sb.AppendLine($"{entry.Username}: {entry.NetKarma}");
        }

        await SendMessageResponseAsync(sb.ToString().Trim(), message);
    }
}