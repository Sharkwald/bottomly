using Bottomly.Configuration;
using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SlackNet;
using SlackNet.WebApi;

namespace Bottomly.Tests.Slack;

public class SlackMessageBrokerTests
{
    private readonly Mock<IMemberRepository> _mockRepo = new();
    private readonly Mock<ISlackApiClient> _mockSlack = new();
    private readonly Mock<IChatApi> _mockChat = new();
    private readonly Mock<IReactionsApi> _mockReactions = new();
    private readonly Mock<IConversationsApi> _mockConversations = new();

    private SlackMessageBroker CreateBroker(string environment = "live") =>
        new(_mockRepo.Object, _mockSlack.Object,
            Options.Create(new BottomlyOptions { Environment = environment }),
            NullLogger<SlackMessageBroker>.Instance);

    public SlackMessageBrokerTests()
    {
        _mockSlack.Setup(s => s.Chat).Returns(_mockChat.Object);
        _mockSlack.Setup(s => s.Reactions).Returns(_mockReactions.Object);
        _mockSlack.Setup(s => s.Conversations).Returns(_mockConversations.Object);
        _mockChat.Setup(c => c.PostMessage(It.IsAny<Message>())).ReturnsAsync(new PostMessageResponse());
        _mockReactions.Setup(r => r.AddToMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task SendMessageAsync_EmptyText_DoesNotPost()
    {
        var broker = CreateBroker();

        await broker.SendMessageAsync("", "C1");

        _mockChat.Verify(c => c.PostMessage(It.IsAny<Message>()), Times.Never());
    }

    [Fact]
    public async Task SendMessageAsync_ValidText_PostsToChannel()
    {
        var broker = CreateBroker();

        await broker.SendMessageAsync("Hello!", "C1");

        _mockChat.Verify(c => c.PostMessage(It.Is<Message>(m =>
            m.Text == "Hello!" && m.Channel == "C1")), Times.Once());
    }

    [Fact]
    public async Task SendMessageAsync_WithReplyTs_SetsThreadTs()
    {
        var broker = CreateBroker();

        await broker.SendMessageAsync("Reply", "C1", "ts123");

        _mockChat.Verify(c => c.PostMessage(It.Is<Message>(m =>
            m.ThreadTs == "ts123")), Times.Once());
    }

    [Fact]
    public async Task SendMessageAsync_DebugMode_PrependsPrefixToText()
    {
        var broker = CreateBroker(environment: "Dev");

        await broker.SendMessageAsync("Hello!", "C1");

        _mockChat.Verify(c => c.PostMessage(It.Is<Message>(m =>
            m.Text!.StartsWith("[Dev]"))), Times.Once());
    }

    [Fact]
    public async Task SendReactionAsync_CallsSlackReactions()
    {
        var broker = CreateBroker();

        await broker.SendReactionAsync("thumbsup", "C1", "ts123");

        _mockReactions.Verify(r => r.AddToMessage("thumbsup", "C1", "ts123"), Times.Once());
    }

    [Fact]
    public async Task SendDmAsync_EmptyText_DoesNotSend()
    {
        var broker = CreateBroker();

        await broker.SendDmAsync("", "alice");

        _mockRepo.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task SendDmAsync_ValidText_OpensConversationAndPosts()
    {
        _mockRepo.Setup(r => r.GetByUsernameAsync("alice"))
            .ReturnsAsync(new Member { Username = "alice", SlackId = "U_ALICE" });
        _mockConversations.Setup(c => c.Open(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("D_CHANNEL");

        var broker = CreateBroker();

        await broker.SendDmAsync("Private message", "alice");

        _mockChat.Verify(c => c.PostMessage(It.Is<Message>(m =>
            m.Channel == "D_CHANNEL" && m.Text == "Private message")), Times.Once());
    }
}
