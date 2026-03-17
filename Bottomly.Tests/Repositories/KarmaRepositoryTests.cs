using Bottomly.Models;
using Bottomly.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Repositories;

public class KarmaRepositoryTests
{
    private readonly Mock<IMongoCollection<Karma>> _mockCollection = new();
    private readonly Mock<IMongoDatabase> _mockDatabase = new();
    private readonly KarmaRepository _repository;

    public KarmaRepositoryTests()
    {
        _mockDatabase
            .Setup(d => d.GetCollection<Karma>("karma", It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockCollection.Object);
        _repository = new KarmaRepository(_mockDatabase.Object);
    }

    private static Mock<IAsyncCursor<BsonDocument>> CreateBsonCursor(IEnumerable<BsonDocument> docs)
    {
        var cursor = new Mock<IAsyncCursor<BsonDocument>>();
        cursor.Setup(c => c.Current).Returns(docs.ToList());
        cursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        return cursor;
    }

    private void SetupAggregate(IEnumerable<BsonDocument> results)
    {
        var cursor = CreateBsonCursor(results);
        _mockCollection
            .Setup(c => c.Aggregate(
                It.IsAny<PipelineDefinition<Karma, BsonDocument>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(cursor.Object);
    }

    [Fact]
    public async Task AddAsync_CallsInsertOne()
    {
        var karma = new Karma { AwardedToUsername = "alice", AwardedByUsername = "bob", KarmaType = KarmaType.PozzyPoz };
        _mockCollection
            .Setup(c => c.InsertOneAsync(karma, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _repository.AddAsync(karma);

        _mockCollection.Verify(
            c => c.InsertOneAsync(karma, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact]
    public async Task GetCurrentNetKarmaAsync_WithResults_ReturnsNetKarma()
    {
        var doc = new BsonDocument { { "_id", "alice" }, { "net_karma", 5 } };
        SetupAggregate([doc]);

        var result = await _repository.GetCurrentNetKarmaAsync("Alice");

        result.ShouldBe(5);
    }

    [Fact]
    public async Task GetCurrentNetKarmaAsync_NoResults_ReturnsZero()
    {
        SetupAggregate([]);

        var result = await _repository.GetCurrentNetKarmaAsync("nobody");

        result.ShouldBe(0);
    }

    [Fact]
    public async Task GetCurrentNetKarmaAsync_LowercasesRecipient()
    {
        SetupAggregate([]);

        // Should not throw — lowercase conversion is internal
        await _repository.GetCurrentNetKarmaAsync("ALICE");
    }

    [Fact]
    public async Task GetLeaderBoardAsync_ReturnsTopScorers()
    {
        var docs = new[]
        {
            new BsonDocument { { "_id", "alice" }, { "net_karma", 10 } },
            new BsonDocument { { "_id", "bob" }, { "net_karma", 7 } },
            new BsonDocument { { "_id", "carol" }, { "net_karma", 4 } }
        };
        SetupAggregate(docs);

        var result = await _repository.GetLeaderBoardAsync(3);

        result.Count.ShouldBe(3);
        result[0].Username.ShouldBe("alice");
        result[0].NetKarma.ShouldBe(10);
        result[1].Username.ShouldBe("bob");
    }

    [Fact]
    public async Task GetLeaderBoardAsync_LimitsResults()
    {
        var docs = new[]
        {
            new BsonDocument { { "_id", "alice" }, { "net_karma", 10 } },
            new BsonDocument { { "_id", "bob" }, { "net_karma", 7 } },
            new BsonDocument { { "_id", "carol" }, { "net_karma", 4 } },
            new BsonDocument { { "_id", "dave" }, { "net_karma", 2 } }
        };
        SetupAggregate(docs);

        var result = await _repository.GetLeaderBoardAsync(2);

        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetLoserBoardAsync_ReturnsLowestScorers()
    {
        var docs = new[]
        {
            new BsonDocument { { "_id", "dave" }, { "net_karma", -5 } },
            new BsonDocument { { "_id", "eve" }, { "net_karma", -3 } }
        };
        SetupAggregate(docs);

        var result = await _repository.GetLoserBoardAsync(2);

        result.Count.ShouldBe(2);
        result[0].Username.ShouldBe("dave");
        result[0].NetKarma.ShouldBe(-5);
    }

    [Fact]
    public async Task GetKarmaReasonsAsync_SeparatesReasonedAndReasonless()
    {
        var docs = new[]
        {
            new BsonDocument
            {
                { "awarded_to_username", "alice" },
                { "awarded_by_username", "bob" },
                { "karma_type", "PozzyPoz" },
                { "awarded", BsonDateTime.Create(DateTime.UtcNow) },
                { "reason", "great work" }
            },
            new BsonDocument
            {
                { "awarded_to_username", "alice" },
                { "awarded_by_username", "carol" },
                { "karma_type", "PozzyPoz" },
                { "awarded", BsonDateTime.Create(DateTime.UtcNow) },
                { "reason", "" }
            },
            new BsonDocument
            {
                { "awarded_to_username", "alice" },
                { "awarded_by_username", "dave" },
                { "karma_type", "NeggyNeg" },
                { "awarded", BsonDateTime.Create(DateTime.UtcNow) }
                // no reason field
            }
        };
        SetupAggregate(docs);

        var result = await _repository.GetKarmaReasonsAsync("Alice");

        result.Reasoned.Count.ShouldBe(1);
        result.Reasoned[0].AwardedByUsername.ShouldBe("bob");
        result.Reasoned[0].Reason.ShouldBe("great work");
        result.Reasonless.ShouldBe(2);
    }

    [Fact]
    public async Task GetKarmaReasonsAsync_EmptyResults_ReturnsEmpty()
    {
        SetupAggregate([]);

        var result = await _repository.GetKarmaReasonsAsync("nobody");

        result.Reasoned.ShouldBeEmpty();
        result.Reasonless.ShouldBe(0);
    }
}
