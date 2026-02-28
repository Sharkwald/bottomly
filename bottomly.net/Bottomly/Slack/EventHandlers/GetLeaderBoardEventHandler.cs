using System.Text;
using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class GetLeaderBoardEventHandler(
    GetLeaderBoardCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GetLeaderBoardEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Get Leaderboard";
    public override ICommand Command => command;
    protected override string CommandSymbol => "leaderboard";
    public override string GetUsage() => CommandTrigger + "[size of leaderboard. Default is 3]";

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandTrigger.TrimEnd()) == true;

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