using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class TestHandler(
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<TestHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Test";
    protected override string CommandSymbol => "test";

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message) =>
        await Broker.SendMessageAsync("OK", message.Channel);
}