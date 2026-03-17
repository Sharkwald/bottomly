using Bottomly.LlmBot;
using Bottomly.Models;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;

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