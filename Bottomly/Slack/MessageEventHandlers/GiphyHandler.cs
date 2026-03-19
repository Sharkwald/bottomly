using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Blocks;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class GiphyHandler(
    GiphyCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GiphyHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Giphy";
    protected override ICommand Command => command;
    protected override string CommandSymbol => "gif";
    protected override string GetUsage() => CommandTrigger + "<search term>";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var term = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(term);

        if (result is GiphySuccessResult success)
        {
            var blocks = new List<Block>
            {
                new ImageBlock
                {
                    ImageUrl = success.Url,
                    AltText = term,
                    Title = new PlainText { Text = term }
                }
            };
            await SendBlocksResponseAsync(blocks, message, term);
        }
        else
        {
            await SendMessageResponseAsync($"No gifs found for \"{term}\"", message);
        }
    }
}