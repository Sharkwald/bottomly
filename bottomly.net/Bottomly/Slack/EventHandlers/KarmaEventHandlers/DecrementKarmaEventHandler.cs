using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bottomly.Slack.EventHandlers.KarmaEventHandlers;

public class DecrementKarmaEventHandler(
    AddKarmaCommand command,
    SlackParser parser,
    IMemberRepository memberRepository,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<DecrementKarmaEventHandler> logger)
    : AbstractKarmaEventHandler(command, parser, memberRepository, broker, options, logger)
{
    public override string Name => "Neggy-neg";
    protected override string CommandSymbol => "--";
    protected override KarmaType KarmaTypeValue => KarmaType.NeggyNeg;
}