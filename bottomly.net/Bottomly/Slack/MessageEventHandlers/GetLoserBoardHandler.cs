using System.Text;
using Bottomly.Configuration;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class GetLoserBoardHandler(
    IKarmaRepository repository,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GetLoserBoardHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Get Loserboard";
    protected override string CommandSymbol => "loserboard";
    protected override string GetPurpose() => "Shows the worst of the worst!";
    protected override string GetUsage() => CommandTrigger + "[size of loserboard. Default is 3]";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var sizeArg = message.Text![CommandTrigger.Length..];
        var size = int.TryParse(sizeArg, out var parsed) && parsed > 0 ? parsed : 3;

        var result = await repository.GetLoserBoardAsync(size);
        var sb = new StringBuilder();
        foreach (var entry in result)
        {
            sb.AppendLine($"{entry.Username}: {entry.NetKarma}");
        }

        await SendMessageResponseAsync(sb.ToString().Trim(), message);
    }
}