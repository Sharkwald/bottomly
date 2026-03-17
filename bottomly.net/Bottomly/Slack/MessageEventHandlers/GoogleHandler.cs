using Bottomly.Commands;
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
    protected override string GetUsage() => CommandTrigger + "<query>";

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