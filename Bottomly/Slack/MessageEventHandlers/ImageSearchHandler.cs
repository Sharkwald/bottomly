using Bottomly.Commands;
using Bottomly.Commands.Search;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Blocks;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class ImageSearchHandler(
    ImageSearchCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<ImageSearchHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Image Search";
    protected override ICommand Command => command;
    protected override string CommandSymbol => "gi";

    protected override string GetUsage() => CommandTrigger + "<query>";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var query = message.Text![CommandTrigger.Length..];
        var result = await command.ExecuteAsync(query);

        if (result is SearchResult success)
        {
            var blocks = new List<Block>
            {
                new ImageBlock
                {
                    ImageUrl = success.Link,
                    AltText = success.Title,
                    Title = new PlainText { Text = success.Title }
                }
            };
            await SendBlocksResponseAsync(blocks, message, success.Title);
        }
        else
        {
            await SendMessageResponseAsync($"No image results found for \"{query}\"", message);
        }
    }
}