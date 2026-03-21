using Bottomly.LlmBot;
using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;
using Moq;
using Shouldly;
using SlackNet.Events;

namespace Bottomly.Tests.Slack.MessageEventHandlers.ConversationMessageHandling;

public class MessageContextExtensionsTests
{
    private readonly SlackParser _parser = new(new Mock<IMemberRepository>().Object);

    [Fact]
    public void CreateFromMember_MapsAllProperties()
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
        note.FullName.ShouldBe("Alice Smith");
        note.Gender.ShouldBe(Gender.Female);
        note.SassLevel.ShouldBe(SassLevel.Moderate);
        note.MiscInfo.ShouldBe("Loves gardening");
    }

    [Fact]
    public async Task CreateFromSlackMessage_KnownUser_TranslatesUsername()
    {
        var message = new MessageEvent { User = "U123", Text = "hello there" };
        var memberLookup = new Dictionary<string, string> { ["U123"] = "alice" };

        var inputMessage = await BottomlyInputMessage.CreateFromSlackMessage(message, memberLookup, _parser);

        inputMessage.Username.ShouldBe("alice");
        inputMessage.Text.ShouldBe("hello there");
    }

    [Fact]
    public async Task CreateFromSlackMessage_UnknownUser_FallsBackToSlackId()
    {
        var message = new MessageEvent { User = "U_UNKNOWN", Text = "hey" };
        var memberLookup = new Dictionary<string, string>();

        var inputMessage = await BottomlyInputMessage.CreateFromSlackMessage(message, memberLookup, _parser);

        inputMessage.Username.ShouldBe("U_UNKNOWN");
        inputMessage.Text.ShouldBe("hey");
    }
}