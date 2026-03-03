using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class MemberJoinedMessageEventHandler(
    AddMemberCommand command,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    public override string Name => "Add Member";
    public override ICommand? Command => command;
    protected override string CommandSymbol => string.Empty;
    protected override Task InvokeHandlerLogicAsync(MessageEvent message) => throw new NotImplementedException();
}