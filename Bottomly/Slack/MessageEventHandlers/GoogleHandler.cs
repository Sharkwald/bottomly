using Bottomly.Commands;
using Bottomly.Commands.Google;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class GoogleHandler(
    GoogleSearchCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GoogleHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Google";
    protected override ICommand Command => command;
    protected override string CommandSymbol => "g";

    protected override string GetUsage()
    {
        return CommandTrigger + "<query>";
    }

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var query = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(query);

        var response = result switch
        {
            GoogleSearchResult success => $"{success.Title} {success.Link}",
            EmptySearchTermErrorResult => $"No results found for \"{query}\"",
            _ => "Left as an exercise for the reader."
        };

        await SendMessageResponseAsync(response, message);
    }
}