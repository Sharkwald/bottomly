using Bottomly.Commands;
using Bottomly.Configuration;
using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.EventHandlers.KarmaEventHandlers;

public abstract class AbstractKarmaEventHandler(
    AddKarmaCommand command,
    SlackParser parser,
    IMemberRepository memberRepository,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger logger)
    : AbstractEventHandler(broker, options, logger)
{
    private const string ForString = " for ";
    protected readonly AddKarmaCommand KarmaCommand = command;

    public override ICommand Command => KarmaCommand;
    public abstract override string Name { get; }
    protected abstract KarmaType KarmaTypeValue { get; }

    public override string GetUsage() => CommandSymbol + " recipient [[for <if recipient is not a known user>] reason]";

    protected override bool IsHelpEvent(MessageEvent message) =>
        message.Text?.Trim() == CommandSymbol + " -?";

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandSymbol) == true;

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var args = await ParseCommandTextAsync(message.Text!);
        await KarmaCommand.ExecuteAsync(
            args.Recipient,
            message.User,
            args.Reason,
            args.KarmaType);
        await SendReactionResponseAsync(message);
    }

    private async Task<KarmaArgs> ParseCommandTextAsync(string commandText)
    {
        commandText = await parser.ReplaceSlackIdTokensWithUsernamesAsync(commandText);
        commandText = commandText[CommandSymbol.Length..].TrimStart();

        var split = commandText.Split(ForString, 2);
        string recipient;
        if (split.Length > 1)
        {
            recipient = split[0].Trim();
        }
        else
        {
            recipient = await ParseRecipientAsync(commandText);
        }

        var reason = commandText.Replace(recipient, string.Empty, StringComparison.Ordinal);
        if (reason.StartsWith(ForString))
        {
            reason = reason[ForString.Length..].TrimStart();
        }
        else
        {
            reason = reason.TrimStart();
        }

        return new KarmaArgs(recipient, reason, KarmaTypeValue);
    }

    private async Task<string> ParseRecipientAsync(string commandText)
    {
        var possibleUsername = commandText.Split(' ')[0];
        var member = await memberRepository.GetByUsernameAsync(possibleUsername);
        return member is not null ? possibleUsername : commandText;
    }

    private record KarmaArgs(string Recipient, string Reason, KarmaType KarmaType);
}