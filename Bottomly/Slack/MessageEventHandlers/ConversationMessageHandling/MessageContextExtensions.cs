using Bottomly.LlmBot;
using Bottomly.Models;
using SlackNet.Events;

namespace Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;

internal static class MessageContextExtensions
{
    extension(BottomlyUserNote bottomlyUserNote)
    {
        public static BottomlyUserNote CreateFromMember(Member member) =>
            BottomlyUserNote.Create(member.Username, member.FullName, member.Gender, member.SassLevel, member.MiscInfo);
    }

    extension(BottomlyInputMessage bottomlyInputMessage)
    {
        public static async Task<BottomlyInputMessage> CreateFromSlackMessage(MessageEvent message,
            IDictionary<string, string> memberLookup, SlackParser parser)
        {
            var translatedUserName = memberLookup.TryGetValue(message.User, out var username) ? username : message.User;
            var parsedText = await parser.ReplaceSlackIdTokensWithUsernamesAsync(message.Text);
            return BottomlyInputMessage.Create(translatedUserName, parsedText);
        }
    }

    extension(IEnumerable<MessageEvent> slackMessages)
    {
        public async Task<IList<BottomlyInputMessage>> ToInputMessagesAsync(IDictionary<string, string> memberLookup,
            SlackParser parser)
        {
            List<BottomlyInputMessage> contextMessages = [];
            foreach (var h in slackMessages.OrderBy(h => h.Timestamp))
            {
                contextMessages.Add(await BottomlyInputMessage.CreateFromSlackMessage(h, memberLookup, parser));
            }

            return contextMessages;
        }
    }
}