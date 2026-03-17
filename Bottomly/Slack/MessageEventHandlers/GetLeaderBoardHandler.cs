using System.Text;
using Bottomly.Configuration;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class GetLeaderBoardHandler(
    IKarmaRepository repository,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GetLeaderBoardHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Get Leaderboard";
    protected override string CommandSymbol => "leaderboard";
    protected override string GetPurpose() => "Shows the best of the best!";
    protected override string GetUsage() => CommandTrigger + "[size of leaderboard. Default is 3]";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var sizeArg = message.Text![CommandTrigger.Trim().Length..];
        var size = int.TryParse(sizeArg, out var parsed) && parsed > 0 ? parsed : 3;

        var result = await repository.GetLeaderBoardAsync(size);
        var sb = new StringBuilder();
        foreach (var entry in result)
        {
            sb.AppendLine($"{entry.Username}: {entry.NetKarma}");
        }

        await SendMessageResponseAsync(sb.ToString().Trim(), message);
    }
}