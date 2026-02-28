using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class UrbanEventHandler(
    UrbanSearchCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<UrbanEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Urban Dictionary";
    public override ICommand Command => command;
    protected override string CommandSymbol => "ud";
    public override string GetUsage() => CommandTrigger + "<term>";

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandTrigger) == true;

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var term = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(term);
        var response = result ?? "Left as an exercise for the reader.";
        await SendMessageResponseAsync(response, message, true);
    }
}