using Bottomly.LlmBot;
using Bottomly.Models;
using Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.MessageEventHandlers.ConversationMessageHandling;

public class MessageContextExtensionsTests
{
    [Fact]
    public void CreateFromMember_MapsUsernameAndNote()
    {
        var member = new Member
        {
            Username = "alice",
            FullName = "Alice Smith",
            Gender = Gender.Female,
            SassLevel = SassLevel.Moderate,
            MiscInfo = "Loves gardening"
        };

        var note = BottomlyUserNote.CreateFromMember(member);

        note.Username.ShouldBe("alice");
        note.Note.ShouldContain("Alice Smith");
    }

    [Fact]
    public void CreateFromSlackMessage_KnownUser_TranslatesUsername()
    {
        var message = new MessageEvent { User = "U123", Text = "hello there" };
        var memberLookup = new Dictionary<string, string> { ["U123"] = "alice" };

        var inputMessage = BottomlyInputMessage.CreateFromSlackMessage(message, memberLookup);

        inputMessage.Username.ShouldBe("alice");
        inputMessage.Text.ShouldBe("hello there");
    }

    [Fact]
    public void CreateFromSlackMessage_UnknownUser_FallsBackToSlackId()
    {
        var message = new MessageEvent { User = "U_UNKNOWN", Text = "hey" };
        var memberLookup = new Dictionary<string, string>();

        var inputMessage = BottomlyInputMessage.CreateFromSlackMessage(message, memberLookup);

        inputMessage.Username.ShouldBe("U_UNKNOWN");
        inputMessage.Text.ShouldBe("hey");
    }
}
