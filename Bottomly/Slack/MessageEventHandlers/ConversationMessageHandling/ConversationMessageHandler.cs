using Bottomly.LlmBot;
using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;

public class ConversationMessageHandler(
    ILlmClient llmMessageBroker,
    ISlackMessageBroker slackBroker,
    ISlackApiClient apiClient,
    IMemberRepository memberRepository,
    IFeatureFlagRepository featureFlagRepository,
    ILogger<ConversationMessageHandler> logger
) : IMessageEventHandler
{
    private readonly Task<Member?> _botMemberTask = memberRepository.GetByUsernameAsync("bottomly");

    public bool CanHandle(MessageEvent message)
    {
        if (message.Text.Contains("bottomly")) return true;
        return _botMemberTask.IsCompletedSuccessfully
            && _botMemberTask.Result?.SlackId is { } botId
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

        var contextMessages = history.Messages
            .OrderBy(h => h.Timestamp)
            .Select(h => BottomlyInputMessage.CreateFromSlackMessage(h, memberLookup))
            .ToList();

        var userNotes = contextMembers.Select(BottomlyUserNote.CreateFromMember).ToList();

        var mainPrompt = BottomlyInputMessage.CreateFromSlackMessage(message, memberLookup);

        var context = MessageHistoryContext.Create(contextMessages, userNotes);

        var response = await llmMessageBroker.Respond(mainPrompt, context);

        var replyToTs = response.IsError() ? message.TsForReply() : null;

        await slackBroker.SendMessageAsync(response.ToSlackResponse(), message.Channel, replyToTs);
    }

    public string BuildHelpMessage() => string.Empty;
}