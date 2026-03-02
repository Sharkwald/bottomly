using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class RegEventHandler(
    RegSearchCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<RegEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Reg Lookup";
    public override ICommand Command => command;
    protected override string CommandSymbol => "reg";
    protected override string GetUsage() => CommandTrigger + "<registration plate>";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var plate = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(plate);
        await SendMessageResponseAsync(result, message);
    }
}