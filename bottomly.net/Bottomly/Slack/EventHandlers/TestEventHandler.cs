using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers;

public class TestEventHandler(
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<TestEventHandler> logger) : AbstractEventHandler(broker, options, logger)
{
    public override string Name => "Test";
    public override ICommand? Command => NoneCommand.None;
    protected override string CommandSymbol => "test";

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandTrigger.TrimEnd()) ?? false;

    protected override Task InvokeHandlerLogicAsync(MessageEvent message) =>
        Broker.SendMessageAsync("OK", message.Channel);

    public override string GetUsage() => CommandTrigger;
}