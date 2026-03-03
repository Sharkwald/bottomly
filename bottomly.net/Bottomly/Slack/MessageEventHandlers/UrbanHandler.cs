using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class UrbanHandler(
    UrbanSearchCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<UrbanHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Urban Dictionary";
    protected override ICommand Command => command;
    protected override string CommandSymbol => "ud";
    protected override string GetUsage() => CommandTrigger + "<term>";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var term = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(term);
        var response = result ?? "Left as an exercise for the reader.";
        await SendMessageResponseAsync(response, message, true);
    }
}