using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class GiphyEventHandler(
    GiphyCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GiphyEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Giphy";
    public override ICommand Command => command;
    protected override string CommandSymbol => "gif";
    public override string GetUsage() => CommandTrigger + "<search term>";

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandTrigger) == true;

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var term = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(term);
        var response = result ?? $"No gifs found for \"{term}\"";
        await SendMessageResponseAsync(response, message);
    }
}