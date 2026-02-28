using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class WikipediaEventHandler(
    WikipediaSearchCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<WikipediaEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Wikipedia";
    public override ICommand Command => command;
    protected override string CommandSymbol => "wik";
    public override string GetUsage() => CommandTrigger + "<term>";

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandTrigger) == true;

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var term = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(term);
        var response = result is null
            ? $"No results found for \"{term}\""
            : $"{result.Text} {result.Link}";
        await SendMessageResponseAsync(response, message);
    }
}