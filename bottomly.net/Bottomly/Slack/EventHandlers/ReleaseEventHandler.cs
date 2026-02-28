using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class ReleaseEventHandler(
    ReleaseCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<ReleaseEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Release";
    public override ICommand Command => command;
    protected override string CommandSymbol => "release";
    public override string GetUsage() => CommandTrigger.TrimEnd();

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandTrigger.TrimEnd()) == true;

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var result = await command.ExecuteAsync();
        var response = result ?? "Unable to retrieve latest release info.";
        await SendMessageResponseAsync(response, message);
    }
}