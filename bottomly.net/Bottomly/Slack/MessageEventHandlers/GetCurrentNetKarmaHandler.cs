using Bottomly.Configuration;
using Bottomly.Repositories;
using Bottomly.Slack.ReactionHandlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class GetCurrentNetKarmaHandler(
    IKarmaRepository repository,
    KarmaReactionMap reactionMap,
    SlackParser parser,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<GetCurrentNetKarmaHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Get Current Karma";
    protected override string CommandSymbol => "karma";
    protected override string GetUsage() => CommandTrigger + "[recipient <if blank, will default to you>]";

    protected override string GetPurpose() =>
        "Returns someone's/something's current score of imaginary internet points.";

    public override string GetUsageAddendum() => reactionMap.KarmaReactionDescriptions();

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var text = await parser.ReplaceSlackIdTokensWithUsernamesAsync(message.Text!);
        var recipient = text[CommandTrigger.Length..].Split(' ')[0];
        if (string.IsNullOrEmpty(recipient))
        {
            recipient = message.User;
        }

        var result = await repository.GetCurrentNetKarmaAsync(recipient);
        await SendMessageResponseAsync($"{recipient}: {result}", message);
    }
}