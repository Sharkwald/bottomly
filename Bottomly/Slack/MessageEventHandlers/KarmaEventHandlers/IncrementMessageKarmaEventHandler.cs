using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Slack.MessageEventHandlers.KarmaEventHandlers;

public class IncrementMessageKarmaEventHandler(
    AddKarmaCommand command,
    SlackParser parser,
    IMemberRepository memberRepository,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<IncrementMessageKarmaEventHandler> logger)
    : AbstractMessageKarmaEventHandler(command, parser, memberRepository, broker, options, logger)
{
    public override string Name => "Pozzy-poz";
    protected override string CommandSymbol => "++";
    protected override KarmaType KarmaTypeValue => KarmaType.PozzyPoz;
}