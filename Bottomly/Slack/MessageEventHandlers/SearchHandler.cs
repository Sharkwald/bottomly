using Bottomly.Commands;
using Bottomly.Commands.Search;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class SearchHandler(
    SearchCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<SearchHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Web Search";
    protected override ICommand Command => command;
    protected override string CommandSymbol => "g";

    protected override string GetUsage() => CommandTrigger + "<query>";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var query = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(query);

        var response = result switch
        {
            SearchResult success => $"<{success.Link}|{success.Title}>",
            EmptySearchTermErrorResult => $"No results found for \"{query}\"",
            _ => "Left as an exercise for the reader."
        };

        await SendMessageResponseAsync(response, message);
    }
}