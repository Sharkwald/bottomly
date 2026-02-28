using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class GoogleEventHandler(
    GoogleSearchCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GoogleEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Google";
    public override ICommand Command => command;
    protected override string CommandSymbol => "g";
    public override string GetUsage() => CommandTrigger + "<query>";

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandTrigger) == true;

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var query = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(query);
        var response = result is null
            ? $"No results found for \"{query}\""
            : $"{result.Title} {result.Link}";
        await SendMessageResponseAsync(response, message);
    }
}