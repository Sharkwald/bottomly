using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class WikipediaHandler(
    WikipediaSearchCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<WikipediaHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Wikipedia";
    public override ICommand Command => command;
    protected override string CommandSymbol => "wik";
    protected override string GetUsage() => CommandTrigger + "<term>";

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