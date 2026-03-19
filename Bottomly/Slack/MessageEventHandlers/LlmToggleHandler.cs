using Bottomly.Configuration;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

public class LlmToggleHandler(
    IFeatureFlagRepository featureFlagRepository,
    IMemberRepository memberRepository,
    ISlackMessageBroker broker,
    IOptions<BottomlyOptions> options,
    ILogger<LlmToggleHandler> logger)
    : AbstractMessageEventHandler(broker, options, logger)
{
    private const string AuthorisedUser = "owen";

    public override string Name => "LLM Toggle";
    protected override string CommandSymbol => "llm";
    protected override string GetPurpose() => "Enables or disables the LLM at runtime.";
    protected override string GetUsage() => $"{CommandTrigger.TrimEnd()} on|off";

    public override bool CanHandle(MessageEvent message) =>
        message.Text?.StartsWith(CommandTrigger.TrimEnd()) == true;

    protected override async Task InvokeHandlerLogicAsync(MessageEvent message)
    {
        var sender = await memberRepository.GetBySlackIdAsync(message.User);
        if (sender?.Username != AuthorisedUser)
        {
            await SendMessageResponseAsync("You don't have permission to toggle the LLM.", message, true);
            return;
        }

        var arg = message.Text![CommandTrigger.TrimEnd().Length..].Trim().ToLowerInvariant();
        switch (arg)
        {
            case "on":
                await featureFlagRepository.SetAsync("EnableLlm", true);
                await SendMessageResponseAsync("LLM enabled.", message, true);
                break;
            case "off":
                await featureFlagRepository.SetAsync("EnableLlm", false);
                await SendMessageResponseAsync("LLM disabled.", message, true);
                break;
            default:
                await SendMessageResponseAsync($"Unknown argument '{arg}'. Use `{GetUsage()}`.", message, true);
                break;
        }
    }
}