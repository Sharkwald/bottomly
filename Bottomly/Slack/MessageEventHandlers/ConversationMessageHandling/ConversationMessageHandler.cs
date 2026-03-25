using Bottomly.LlmBot;
using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;

public class ConversationMessageHandler(
    ILlmClient llmClient,
    ISlackMessageBroker slackBroker,
    SlackParser parser,
    ISlackApiClient apiClient,
    IMemberRepository memberRepository,
    IFeatureFlagRepository featureFlagRepository,
    ILogger<ConversationMessageHandler> logger
) : IMessageEventHandler
{
    private readonly Task<Member?> _botMemberTask = memberRepository.GetByUsernameAsync("bottomly");

    public bool CanHandle(MessageEvent message)
    {
        if (message.ChannelType == "im")
        {
            return true;
        }

        if (message.Text.Contains("bottomly", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return _botMemberTask is { IsCompletedSuccessfully: true, Result.SlackId: { } botId }
               && message.Text.Contains($"<@{botId}>");
    }

    public async Task HandleAsync(MessageEvent message)
    {
        logger.LogInformation("Handling conversation message from {User} in {Channel}", message.User, message.Channel);

        if (!await featureFlagRepository.GetAsync("EnableLlm"))
        {
            logger.LogInformation("LLM is disabled; skipping conversation handling.");
            return;
        }

        var history = await apiClient.Conversations.History(message.Channel, limit: 11);

        var contextUsersSlackIds = history.Messages.Select(m => m.User).Distinct();
        var contextMembers = await memberRepository.GetBySlackIdsAsync(contextUsersSlackIds.Union([message.User]));
        var memberLookup = contextMembers.ToDictionary(m => m.SlackId, m => m.Username);

        var contextMessages = await history.Messages.ToInputMessagesAsync(memberLookup, parser);

        var userNotes = contextMembers.Select(BottomlyUserNote.CreateFromMember).ToList();

        var mainPrompt = await BottomlyInputMessage.CreateFromSlackMessage(message, memberLookup, parser);

        var context = MessageHistoryContext.Create(contextMessages, userNotes);

        var response = await llmClient.Respond(mainPrompt, context);

        var replyToTs = message.ThreadTs != null || response.IsError() ? message.TsForReply() : null;

        await slackBroker.SendMessageAsync(response.ToSlackResponse(), message.Channel, replyToTs);
    }

    public string BuildHelpMessage() => string.Empty;
}