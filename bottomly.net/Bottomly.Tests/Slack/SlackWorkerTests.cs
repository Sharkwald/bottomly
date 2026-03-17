using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Slack;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Slack.ReactionHandlers;
using Bottomly.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SlackNet;
using SlackNet.Events;
using SlackNet.SocketMode;

namespace Bottomly.Tests.Slack;

public class SlackWorkerTests
{
    private readonly Mock<ISlackSocketModeClient> _mockSocket = new();
    private readonly Mock<ISlackMessageBroker> _mockBroker = new();
    private readonly Mock<IMemberRepository> _mockMemberRepo = new();

    private SlackWorker CreateWorker(
        IEnumerable<IMessageEventHandler>? handlers = null,
        IEnumerable<IReactionHandler>? reactionHandlers = null)
    {
        var options = TestHelpers.CreateOptions();
        var helpHandler = new HelpHandler(
            handlers ?? [],
            _mockBroker.Object,
            options,
            NullLogger<HelpHandler>.Instance);

        return new SlackWorker(
            _mockSocket.Object,
            handlers ?? [],
            helpHandler,
            reactionHandlers ?? [],
            _mockMemberRepo.Object,
            NullLogger<SlackWorker>.Instance);
    }

    private static MessageEvent CreateMessage(string text, string user = "U1", string? botId = null) =>
        new() { Text = text, User = user, Channel = "C1", Ts = "ts1", BotId = botId };

    // ── ExecuteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ConnectsToSocketClient()
    {
        var connectCalled = new TaskCompletionSource();
        _mockSocket
            .Setup(s => s.Connect(It.IsAny<SocketModeConnectionOptions>(), It.IsAny<CancellationToken>()))
            .Callback(() => connectCalled.TrySetResult())
            .Returns(Task.CompletedTask);

        var worker = CreateWorker();
        await worker.StartAsync(CancellationToken.None);

        await connectCalled.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _mockSocket.Verify(
            s => s.Connect(It.IsAny<SocketModeConnectionOptions>(), It.IsAny<CancellationToken>()),
            Times.Once());

        await worker.StopAsync(CancellationToken.None);
    }

    // ── ProcessMessageAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ProcessMessageAsync_EmptyText_DoesNotInvokeHandlers()
    {
        var mockHandler = new Mock<IMessageEventHandler>();
        var worker = CreateWorker([mockHandler.Object]);

        await worker.ProcessMessageAsync(CreateMessage(""));

        mockHandler.Verify(h => h.CanHandle(It.IsAny<MessageEvent>()), Times.Never());
    }

    [Fact]
    public async Task ProcessMessageAsync_BotMessage_DoesNotInvokeHandlers()
    {
        var mockHandler = new Mock<IMessageEventHandler>();
        var worker = CreateWorker([mockHandler.Object]);

        await worker.ProcessMessageAsync(CreateMessage("hello", botId: "B1"));

        mockHandler.Verify(h => h.CanHandle(It.IsAny<MessageEvent>()), Times.Never());
    }

    [Fact]
    public async Task ProcessMessageAsync_MatchingHandler_InvokesHandler()
    {
        var mockHandler = new Mock<IMessageEventHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<MessageEvent>())).Returns(true);
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<MessageEvent>())).Returns(Task.CompletedTask);
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync(It.IsAny<string>())).ReturnsAsync((Member?)null);

        var worker = CreateWorker([mockHandler.Object]);
        await worker.ProcessMessageAsync(CreateMessage("_wiki cats"));

        mockHandler.Verify(h => h.HandleAsync(It.IsAny<MessageEvent>()), Times.Once());
    }

    [Fact]
    public async Task ProcessMessageAsync_StopsAtFirstMatchingHandler()
    {
        var mockHandler1 = new Mock<IMessageEventHandler>();
        mockHandler1.Setup(h => h.CanHandle(It.IsAny<MessageEvent>())).Returns(true);
        mockHandler1.Setup(h => h.HandleAsync(It.IsAny<MessageEvent>())).Returns(Task.CompletedTask);

        var mockHandler2 = new Mock<IMessageEventHandler>();
        mockHandler2.Setup(h => h.CanHandle(It.IsAny<MessageEvent>())).Returns(true);

        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync(It.IsAny<string>())).ReturnsAsync((Member?)null);

        var worker = CreateWorker([mockHandler1.Object, mockHandler2.Object]);
        await worker.ProcessMessageAsync(CreateMessage("_wiki cats"));

        mockHandler1.Verify(h => h.HandleAsync(It.IsAny<MessageEvent>()), Times.Once());
        mockHandler2.Verify(h => h.HandleAsync(It.IsAny<MessageEvent>()), Times.Never());
    }

    [Fact]
    public async Task ProcessMessageAsync_NoMatchingHandler_DoesNotThrow()
    {
        var mockHandler = new Mock<IMessageEventHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<MessageEvent>())).Returns(false);
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync(It.IsAny<string>())).ReturnsAsync((Member?)null);

        var worker = CreateWorker([mockHandler.Object]);

        // Should complete without throwing
        await worker.ProcessMessageAsync(CreateMessage("unrecognised command"));

        mockHandler.Verify(h => h.HandleAsync(It.IsAny<MessageEvent>()), Times.Never());
    }

    [Fact]
    public async Task ProcessMessageAsync_HelpMessage_RoutesToHelpHandler()
    {
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync(It.IsAny<string>())).ReturnsAsync((Member?)null);
        _mockBroker.Setup(b => b.SendDmAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var mockHandler = new Mock<IMessageEventHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<MessageEvent>())).Returns(false);
        mockHandler.Setup(h => h.BuildHelpMessage()).Returns("some help text");

        var worker = CreateWorker([mockHandler.Object]);
        await worker.ProcessMessageAsync(CreateMessage("_help", user: "U1"));

        _mockBroker.Verify(b => b.SendDmAsync(It.IsAny<string>(), "U1"), Times.Once());
        mockHandler.Verify(h => h.HandleAsync(It.IsAny<MessageEvent>()), Times.Never());
    }

    [Fact]
    public async Task ProcessMessageAsync_ResolvesUsernameFromSlackId()
    {
        var member = new Member { Username = "alice", SlackId = "U1" };
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync("U1")).ReturnsAsync(member);

        string? capturedUser = null;
        var mockHandler = new Mock<IMessageEventHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<MessageEvent>())).Returns(true);
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<MessageEvent>()))
            .Callback<MessageEvent>(m => capturedUser = m.User)
            .Returns(Task.CompletedTask);

        var worker = CreateWorker([mockHandler.Object]);
        await worker.ProcessMessageAsync(CreateMessage("_wiki cats", user: "U1"));

        capturedUser.ShouldBe("alice");
    }

    [Fact]
    public async Task ProcessMessageAsync_UnknownSlackId_LeavesUserUnchanged()
    {
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync("U_UNKNOWN")).ReturnsAsync((Member?)null);

        string? capturedUser = null;
        var mockHandler = new Mock<IMessageEventHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<MessageEvent>())).Returns(true);
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<MessageEvent>()))
            .Callback<MessageEvent>(m => capturedUser = m.User)
            .Returns(Task.CompletedTask);

        var worker = CreateWorker([mockHandler.Object]);
        await worker.ProcessMessageAsync(CreateMessage("_wiki cats", user: "U_UNKNOWN"));

        capturedUser.ShouldBe("U_UNKNOWN");
    }

    [Fact]
    public async Task ProcessMessageAsync_HandlerThrows_DoesNotPropagate()
    {
        _mockMemberRepo.Setup(r => r.GetBySlackIdAsync(It.IsAny<string>())).ReturnsAsync((Member?)null);

        var mockHandler = new Mock<IMessageEventHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<MessageEvent>())).Returns(true);
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<MessageEvent>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var worker = CreateWorker([mockHandler.Object]);

        // Should not throw
        await worker.ProcessMessageAsync(CreateMessage("_wiki cats"));
    }

    // ── ProcessReactionAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ProcessReactionAsync_MatchingHandler_InvokesHandler()
    {
        var mockHandler = new Mock<IReactionHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<ReactionAdded>())).Returns(true);
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<ReactionAdded>())).Returns(Task.CompletedTask);

        var worker = CreateWorker(reactionHandlers: [mockHandler.Object]);
        await worker.ProcessReactionAsync(new ReactionAdded { Reaction = "joy" });

        mockHandler.Verify(h => h.HandleAsync(It.IsAny<ReactionAdded>()), Times.Once());
    }

    [Fact]
    public async Task ProcessReactionAsync_NoMatchingHandler_NoHandlerInvoked()
    {
        var mockHandler = new Mock<IReactionHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<ReactionAdded>())).Returns(false);

        var worker = CreateWorker(reactionHandlers: [mockHandler.Object]);
        await worker.ProcessReactionAsync(new ReactionAdded { Reaction = "robot_face" });

        mockHandler.Verify(h => h.HandleAsync(It.IsAny<ReactionAdded>()), Times.Never());
    }

    [Fact]
    public async Task ProcessReactionAsync_MultipleMatchingHandlers_AllInvoked()
    {
        var mockHandler1 = new Mock<IReactionHandler>();
        mockHandler1.Setup(h => h.CanHandle(It.IsAny<ReactionAdded>())).Returns(true);
        mockHandler1.Setup(h => h.HandleAsync(It.IsAny<ReactionAdded>())).Returns(Task.CompletedTask);

        var mockHandler2 = new Mock<IReactionHandler>();
        mockHandler2.Setup(h => h.CanHandle(It.IsAny<ReactionAdded>())).Returns(true);
        mockHandler2.Setup(h => h.HandleAsync(It.IsAny<ReactionAdded>())).Returns(Task.CompletedTask);

        var worker = CreateWorker(reactionHandlers: [mockHandler1.Object, mockHandler2.Object]);
        await worker.ProcessReactionAsync(new ReactionAdded { Reaction = "joy" });

        mockHandler1.Verify(h => h.HandleAsync(It.IsAny<ReactionAdded>()), Times.Once());
        mockHandler2.Verify(h => h.HandleAsync(It.IsAny<ReactionAdded>()), Times.Once());
    }

    [Fact]
    public async Task ProcessReactionAsync_HandlerThrows_DoesNotPropagate()
    {
        var mockHandler = new Mock<IReactionHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<ReactionAdded>())).Returns(true);
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<ReactionAdded>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var worker = CreateWorker(reactionHandlers: [mockHandler.Object]);

        // Should not throw
        await worker.ProcessReactionAsync(new ReactionAdded { Reaction = "joy" });
    }
}

public class SlackEventDispatcherTests
{
    [Fact]
    public async Task SlackMessageEventDispatcher_DelegatesToWorker()
    {
        var mockSocket = new Mock<ISlackSocketModeClient>();
        var mockBroker = new Mock<ISlackMessageBroker>();
        var mockRepo = new Mock<IMemberRepository>();
        var options = TestHelpers.CreateOptions();
        var helpHandler = new HelpHandler([], mockBroker.Object, options, NullLogger<HelpHandler>.Instance);
        var worker = new SlackWorker(mockSocket.Object, [], helpHandler, [],
            mockRepo.Object, NullLogger<SlackWorker>.Instance);

        var dispatcher = new SlackMessageEventDispatcher(worker);
        var message = new MessageEvent { Text = "", User = "U1", Channel = "C1" };

        // Empty message is a no-op — just verify it doesn't throw
        await dispatcher.Handle(message);
    }

    [Fact]
    public async Task SlackReactionEventDispatcher_DelegatesToWorker()
    {
        var mockSocket = new Mock<ISlackSocketModeClient>();
        var mockBroker = new Mock<ISlackMessageBroker>();
        var mockRepo = new Mock<IMemberRepository>();
        var options = TestHelpers.CreateOptions();
        var helpHandler = new HelpHandler([], mockBroker.Object, options, NullLogger<HelpHandler>.Instance);
        var worker = new SlackWorker(mockSocket.Object, [], helpHandler, [],
            mockRepo.Object, NullLogger<SlackWorker>.Instance);

        var dispatcher = new SlackReactionEventDispatcher(worker);
        await dispatcher.Handle(new ReactionAdded { Reaction = "joy" });
    }
}
