using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class RefreshCacheHandler(
    RefreshCacheCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<RefreshCacheHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Refresh Cache";
    protected override ICommand Command => command;
    protected override string CommandSymbol => "refreshcache";
    protected override string GetUsage() => CommandTrigger.TrimEnd();

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var result = await command.ExecuteAsync();
        var response = result switch
        {
            RefreshCacheSuccessResult success => $"Member cache refreshed. {success.MemberCount} members loaded.",
            _ => "Failed to refresh member cache."
        };
        await SendMessageResponseAsync(response, message);
    }
}
