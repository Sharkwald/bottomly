using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Tests.Infrastructure;
using MongoDB.Driver;
using Shouldly;

namespace Bottomly.Tests.Repositories.Integration;

[Collection("MongoDB")]
public class KarmaRepositoryIntegrationTests(MongoDbFixture fixture) : IAsyncLifetime
{
    private IMongoDatabase _db = null!;
    private KarmaRepository _sut = null!;

    public Task InitializeAsync()
    {
        _db = fixture.GetDatabase($"karma_test_{Guid.NewGuid():N}");
        _sut = new KarmaRepository(_db);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() =>
        await fixture.Client.DropDatabaseAsync(_db.DatabaseNamespace.DatabaseName);

    [Fact]
    public async Task AddAsync_PersistsKarmaDocument()
    {
        var karma = MakeKarma("alice", "bob", KarmaType.PozzyPoz, "great PR");

        await _sut.AddAsync(karma);

        // Verify via GetCurrentNetKarmaAsync — if persisted, aggregate returns a value
        var net = await _sut.GetCurrentNetKarmaAsync("alice");
        net.ShouldNotBe(0);
    }

    [Fact]
    public async Task GetCurrentNetKarmaAsync_WithMixedKarma_ReturnsCorrectNetScore()
    {
        // 2× PozzyPoz = –2, 3× NeggyNeg = +3 → net = +1
        await _sut.AddAsync(MakeKarma("alice", "x", KarmaType.PozzyPoz));
        await _sut.AddAsync(MakeKarma("alice", "x", KarmaType.PozzyPoz));
        await _sut.AddAsync(MakeKarma("alice", "x", KarmaType.NeggyNeg));
        await _sut.AddAsync(MakeKarma("alice", "x", KarmaType.NeggyNeg));
        await _sut.AddAsync(MakeKarma("alice", "x", KarmaType.NeggyNeg));

        var net = await _sut.GetCurrentNetKarmaAsync("alice");

        net.ShouldBe(1);
    }

    [Fact]
    public async Task GetCurrentNetKarmaAsync_IsCaseInsensitive()
    {
        await _sut.AddAsync(MakeKarma("Bob", "x", KarmaType.NeggyNeg));

        var net = await _sut.GetCurrentNetKarmaAsync("bob");

        net.ShouldBe(1);
    }

    [Fact]
    public async Task GetCurrentNetKarmaAsync_WhenNoKarmaExists_ReturnsZero()
    {
        var net = await _sut.GetCurrentNetKarmaAsync("nobody");

        net.ShouldBe(0);
    }

    [Fact]
    public async Task GetCurrentNetKarmaAsync_ExcludesExpiredEntries()
    {
        var expired = MakeKarma("carol", "x", KarmaType.NeggyNeg);
        expired.Awarded = DateTime.UtcNow.AddDays(-(Karma.ExpiryDays + 1));
        await _sut.AddAsync(expired);

        var net = await _sut.GetCurrentNetKarmaAsync("carol");

        net.ShouldBe(0);
    }

    [Fact]
    public async Task GetKarmaReasonsAsync_SeparatesReasonedFromReasonless()
    {
        await _sut.AddAsync(MakeKarma("dave", "x", KarmaType.PozzyPoz, "fixed the build"));
        await _sut.AddAsync(MakeKarma("dave", "x", KarmaType.PozzyPoz, "great docs"));
        await _sut.AddAsync(MakeKarma("dave", "x", KarmaType.NeggyNeg)); // no reason

        var result = await _sut.GetKarmaReasonsAsync("dave");

        result.Reasonless.ShouldBe(1);
        result.Reasoned.Count.ShouldBe(2);
        result.Reasoned.Select(k => k.Reason).ShouldBe(
            ["fixed the build", "great docs"], true);
    }

    [Fact]
    public async Task GetKarmaReasonsAsync_IsCaseInsensitiveOnRecipient()
    {
        await _sut.AddAsync(MakeKarma("Eve", "x", KarmaType.PozzyPoz, "helpful"));

        var result = await _sut.GetKarmaReasonsAsync("eve");

        result.Reasoned.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetKarmaReasonsAsync_ExcludesExpiredEntries()
    {
        var expired = MakeKarma("frank", "x", KarmaType.PozzyPoz, "stale reason");
        expired.Awarded = DateTime.UtcNow.AddDays(-(Karma.ExpiryDays + 1));
        await _sut.AddAsync(expired);

        var result = await _sut.GetKarmaReasonsAsync("frank");

        result.Reasoned.Count.ShouldBe(0);
        result.Reasonless.ShouldBe(0);
    }

    [Fact]
    public async Task GetLeaderBoardAsync_ReturnsCorrectOrderAndSize()
    {
        // net_karma: PozzyPoz → –1, NeggyNeg → +1
        // Leader board is sorted Descending by net_karma
        await AddKarmaMultiple("leader-a", KarmaType.NeggyNeg, 5); // net +5
        await AddKarmaMultiple("leader-b", KarmaType.NeggyNeg, 3); // net +3
        await AddKarmaMultiple("leader-c", KarmaType.NeggyNeg, 1); // net +1
        await AddKarmaMultiple("leader-d", KarmaType.PozzyPoz, 2); // net –2 (should not appear in top 3)

        var board = await _sut.GetLeaderBoardAsync();

        board.Count.ShouldBe(3);
        board[0].Username.ShouldBe("leader-a");
        board[0].NetKarma.ShouldBe(5);
        board[1].Username.ShouldBe("leader-b");
        board[1].NetKarma.ShouldBe(3);
        board[2].Username.ShouldBe("leader-c");
        board[2].NetKarma.ShouldBe(1);
    }

    [Fact]
    public async Task GetLoserBoardAsync_ReturnsCorrectOrderAndSize()
    {
        // Loser board is sorted Ascending by net_karma
        await AddKarmaMultiple("loser-a", KarmaType.PozzyPoz, 5); // net –5
        await AddKarmaMultiple("loser-b", KarmaType.PozzyPoz, 3); // net –3
        await AddKarmaMultiple("loser-c", KarmaType.PozzyPoz, 1); // net –1
        await AddKarmaMultiple("loser-d", KarmaType.NeggyNeg, 2); // net +2 (should not appear in top 3)

        var board = await _sut.GetLoserBoardAsync();

        board.Count.ShouldBe(3);
        board[0].Username.ShouldBe("loser-a");
        board[0].NetKarma.ShouldBe(-5);
        board[1].Username.ShouldBe("loser-b");
        board[1].NetKarma.ShouldBe(-3);
        board[2].Username.ShouldBe("loser-c");
        board[2].NetKarma.ShouldBe(-1);
    }

    [Fact]
    public async Task GetLeaderBoardAsync_DefaultSizeIsThree()
    {
        await AddKarmaMultiple("rank-1", KarmaType.NeggyNeg, 4);
        await AddKarmaMultiple("rank-2", KarmaType.NeggyNeg, 3);
        await AddKarmaMultiple("rank-3", KarmaType.NeggyNeg, 2);
        await AddKarmaMultiple("rank-4", KarmaType.NeggyNeg, 1);

        var board = await _sut.GetLeaderBoardAsync();

        board.Count.ShouldBe(3);
    }

    // ─── helpers ──────────────────────────────────────────────────────────────

    private static Karma MakeKarma(
        string awardedTo, string awardedBy, KarmaType type, string reason = "") =>
        new()
        {
            AwardedToUsername = awardedTo,
            AwardedByUsername = awardedBy,
            KarmaType = type,
            Reason = reason,
            Awarded = DateTime.UtcNow
        };

    private async Task AddKarmaMultiple(string recipient, KarmaType type, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await _sut.AddAsync(MakeKarma(recipient, "giver", type));
        }
    }
}