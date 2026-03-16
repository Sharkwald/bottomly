using Bottomly.LlmBot;
using Bottomly.Repositories;
using SlackNet;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;

public class ConversationMessageHandler(
    LlmMessageBroker llmMessageBroker,
    ISlackMessageBroker slackBroker,
    ISlackApiClient apiClient,
    IMemberRepository memberRepository
) : IMessageEventHandler
{
    public bool CanHandle(MessageEvent message) => message.Text.Contains("bottomly");

    public async Task HandleAsync(MessageEvent message)
    {
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

        await slackBroker.SendMessageAsync(response.ToSlackResponse(), message.Channel);
    }

    public string BuildHelpMessage() => string.Empty;
}