using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public abstract class AbstractMessageEventHandler(
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger logger)
    : IMessageEventHandler
{
    protected readonly ISlackMessageBroker Broker = broker;
    protected readonly ILogger Logger = logger;
    protected readonly string Prefix = options.Value.Prefix;

    public abstract string Name { get; }
    public abstract ICommand? Command { get; }
    protected abstract string CommandSymbol { get; }

    protected string CommandTrigger => Prefix + CommandSymbol + " ";
    public virtual bool CanHandle(MessageEvent message) => message.Text?.StartsWith(CommandTrigger.TrimEnd()) == true;

    public async Task HandleAsync(MessageEvent message)
    {
        Logger.LogInformation("{Handler} handling: {Text}", GetType().Name, message.Text);
        try
        {
            if (IsHelpEvent(message))
            {
                await HandleHelpEventAsync(message);
            }
            else
            {
                await InvokeHandlerLogicAsync(message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling event: {Text}", message.Text);
        }
    }

    public string BuildHelpMessage()
    {
        var name = Name + Environment.NewLine;
        var purpose = Command is not null ? Command.GetPurpose() + Environment.NewLine : string.Empty;
        var usage = $"Usage: `{GetUsage()}`";
        return name + purpose + usage + GetUsageAddendum();
    }

    protected abstract Task InvokeHandlerLogicAsync(MessageEvent message);
    protected virtual string GetUsage() => CommandTrigger.TrimEnd();
    public virtual string GetUsageAddendum() => string.Empty;

    protected virtual bool IsHelpEvent(MessageEvent message) =>
        message.Text?.Trim() == CommandTrigger.TrimEnd() + " -?";

    private async Task HandleHelpEventAsync(MessageEvent message) =>
        await SendMessageResponseAsync(BuildHelpMessage(), message);

    protected async Task SendMessageResponseAsync(string text, MessageEvent message, bool asReply = false)
    {
        string? replyTs = null;
        if (asReply)
        {
            replyTs = message.Ts;
        }

        if (message.ThreadTs is not null)
        {
            replyTs = message.ThreadTs;
        }

        await Broker.SendMessageAsync(text, message.Channel, replyTs);
    }

    protected Task SendReactionResponseAsync(MessageEvent message) =>
        Broker.SendReactionAsync("robot_face", message.Channel, message.Ts);

    protected Task SendDmResponseAsync(string text, MessageEvent message) =>
        Broker.SendDmAsync(text, message.User);
}