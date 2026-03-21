using Bottomly.LlmBot;
using Bottomly.Models;
using Microsoft.Extensions.AI;
using Shouldly;

namespace Bottomly.Tests.LlmBot;

public class BottomlyInputMessageTests
{
    [Fact]
    public void Create_SetsUsernameAndText()
    {
        var msg = BottomlyInputMessage.Create("alice", "hello world");

        msg.Username.ShouldBe("alice");
        msg.Text.ShouldBe("hello world");
    }
}

public class BottomlyUserNoteTests
{
    [Fact]
    public void Create_SetsUsernameAndNote()
    {
        var note = BottomlyUserNote.Create("bob", "likes cats");

        note.Username.ShouldBe("bob");
        note.Note.ShouldBe("likes cats");
    }
}

public class MessageHistoryContextTests
{
    [Fact]
    public void Create_SetsMessageHistoryAndUserNotes()
    {
        var messages = new List<BottomlyInputMessage> { BottomlyInputMessage.Create("alice", "hi") };
        var notes = new List<BottomlyUserNote> { BottomlyUserNote.Create("alice", "some note") };

        var ctx = MessageHistoryContext.Create(messages, notes);

        ctx.MessageHistory.ShouldBe(messages);
        ctx.UserNotes.ShouldBe(notes);
    }

    [Fact]
    public void Create_EmptyLists_ResultsInEmptyContext()
    {
        var ctx = MessageHistoryContext.Create([], []);

        ctx.MessageHistory.ShouldBeEmpty();
        ctx.UserNotes.ShouldBeEmpty();
    }
}

public class LlmResponseExtensionsTests
{
    [Fact]
    public void ToSuccessResponse_ReturnsMsgResponseWithText()
    {
        var chatResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, "Hello there")]);

        var result = chatResponse.ToSuccessResponse();

        result.ShouldBeOfType<LlmMessageResponse>();
        ((LlmMessageResponse)result).Message.ShouldBe("Hello there");
    }

    [Fact]
    public void ToErrorResponse_TimeoutException_ReturnsTimeout()
    {
        var ex = new TimeoutException("timed out");

        var result = ex.ToErrorResponse();

        result.ShouldBeOfType<LlmTimeoutResponse>();
    }

    [Fact]
    public void ToErrorResponse_UsageException_ReturnsUsageExceeded()
    {
        var ex = new Exception("usage limit reached");

        var result = ex.ToErrorResponse();

        result.ShouldBeOfType<LlmUsageExceededResponse>();
    }

    [Fact]
    public void ToErrorResponse_UnknownException_ReturnsUnknownError()
    {
        var ex = new InvalidOperationException("something went wrong");

        var result = ex.ToErrorResponse();

        result.ShouldBeOfType<LlmUnknownErrorResponse>();
    }

    [Fact]
    public void IsSuccess_LlmMessageResponse_ReturnsTrue()
    {
        var chatResponse = new ChatResponse([new ChatMessage(ChatRole.Assistant, "ok")]);
        var response = chatResponse.ToSuccessResponse();

        response.IsError().ShouldBeFalse();
    }

    [Fact]
    public void IsSuccess_LlmTimeoutResponse_ReturnsFalse()
    {
        LlmResponse response = new LlmTimeoutResponse();

        response.IsError().ShouldBeTrue();
    }
}

public class LlmClientExtensionsTests
{
    [Fact]
    public void ToChatContext_IncludesMessageHistoryAndUserInfo()
    {
        var messages = new List<BottomlyInputMessage>
        {
            BottomlyInputMessage.Create("alice", "hello"),
            BottomlyInputMessage.Create("bob", "world")
        };
        var notes = new List<BottomlyUserNote>
        {
            BottomlyUserNote.Create("alice", "likes tea")
        };
        var ctx = MessageHistoryContext.Create(messages, notes);

        var result = ctx.ToChatContext();

        result.ShouldContain("\"message_history\"");
        result.ShouldContain("\"users\"");
        result.ShouldContain("alice");
        result.ShouldContain("hello");
        result.ShouldContain("bob");
        result.ShouldContain("world");
        result.ShouldContain("likes tea");
    }

    [Fact]
    public void ToChatPromptMessage_IncludesUsernameAndText()
    {
        var msg = BottomlyInputMessage.Create("carol", "what time is it?");

        var result = msg.ToChatPromptMessage();

        result.ShouldContain("carol");
        result.ShouldContain("what time is it?");
    }
}

public class FullPromptContextTests
{
    [Fact]
    public void Create_PopulatesHistoryContextAndPromptingMessage()
    {
        var prompt = BottomlyInputMessage.Create("alice", "say something");
        var ctx = MessageHistoryContext.Create([], []);

        var fullCtx = FullPromptContext.Create(prompt, ctx);

        fullCtx.HistoryContext.ShouldNotBeNull();
        fullCtx.PromptingMessage.ShouldNotBeNull();
    }

    [Fact]
    public void ToArray_ContainsThreeMessages()
    {
        var prompt = BottomlyInputMessage.Create("alice", "say something");
        var ctx = MessageHistoryContext.Create([], []);

        var array = FullPromptContext.Create(prompt, ctx).ToArray();

        array.Length.ShouldBe(3);
        array[0].Role.ShouldBe(ChatRole.System);
        array[1].Role.ShouldBe(ChatRole.System);
        array[2].Role.ShouldBe(ChatRole.User);
    }

    [Fact]
    public void SystemPrompt_HasSystemRole() => FullPromptContext.SystemPrompt.Role.ShouldBe(ChatRole.System);

    [Fact]
    public void SystemPrompt_MentionsBottomly() => FullPromptContext.SystemPrompt.Text.ShouldContain("Bottomly");
}

public class MemberNoteTests
{
    [Fact]
    public void Note_ContainsAllMemberInfo()
    {
        var member = new Member
        {
            FullName = "Alice Smith",
            Gender = Gender.Female,
            SassLevel = SassLevel.Frequent,
            MiscInfo = "Drinks tea"
        };

        member.Note.ShouldContain("Alice Smith");
        member.Note.ShouldContain("Female");
        member.Note.ShouldContain("Frequent");
        member.Note.ShouldContain("Drinks tea");
    }
}