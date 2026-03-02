using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class TestEventHandler(
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<TestEventHandler> logger)
    : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Test";
    public override ICommand? Command => ICommand.None;
    protected override string CommandSymbol => "test";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message) =>
        await Broker.SendMessageAsync("OK", message.Channel);
}