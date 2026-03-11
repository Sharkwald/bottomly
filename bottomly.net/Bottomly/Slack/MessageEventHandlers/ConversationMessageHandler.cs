using Bottomly.LlmBot;
using Bottomly.Models;
using Bottomly.Repositories;
using SlackNet;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers;

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

        var llmResponse = await llmMessageBroker.Respond(mainPrompt, context);
        var response = llmResponse.Text;


        await slackBroker.SendMessageAsync(response, message.Channel);
    }

    public string BuildHelpMessage() => string.Empty;
}

internal static class MessageContextExtensions
{
    extension(BottomlyUserNote bottomlyUserNote)
    {
        public static BottomlyUserNote CreateFromMember(Member member) =>
            BottomlyUserNote.Create(member.Username, member.Note);
    }

    extension(BottomlyInputMessage bottomlyInputMessage)
    {
        public static BottomlyInputMessage CreateFromSlackMessage(MessageEvent message,
            IDictionary<string, string> memberLookup)
        {
            var translatedUserName = memberLookup.TryGetValue(message.User, out var username) ? username : message.User;
            return BottomlyInputMessage.Create(translatedUserName, message.Text);
        }
    }
}