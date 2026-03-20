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
    protected override ICommand Command => command;
    protected override string CommandSymbol => "wik";
    protected override string GetUsage() => CommandTrigger + "<term>";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var term = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(term);
        var response = result switch
        {
            WikipediaSuccessResult success => $"<{success.Link}|{success.Text}>",
            WikipediaNotFoundResult => $"No results found for \"{term}\"",
            WikipediaEmptyInputResult => $"No results found for \"{term}\"",
            WikipediaErrorResult => $"No results found for \"{term}\"",
            _ => $"No results found for \"{term}\""
        };
        await SendMessageResponseAsync(response, message);
    }
}