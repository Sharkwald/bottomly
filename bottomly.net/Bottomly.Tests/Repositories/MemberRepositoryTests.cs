using Bottomly.Models;
using Bottomly.Repositories;
using MongoDB.Driver;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Repositories;

public class MemberRepositoryTests
{
    private readonly Mock<IMongoCollection<Member>> _mockCollection = new();
    private readonly Mock<IMongoDatabase> _mockDatabase = new();
    private readonly MemberRepository _repository;

    public MemberRepositoryTests()
    {
        _mockDatabase
            .Setup(d => d.GetCollection<Member>("member", It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockCollection.Object);
        _repository = new MemberRepository(_mockDatabase.Object);
    }

    private Mock<IAsyncCursor<Member>> CreateCursor(IEnumerable<Member> items)
    {
        var cursor = new Mock<IAsyncCursor<Member>>();
        cursor.Setup(c => c.Current).Returns(items.ToList());
        cursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        return cursor;
    }

    private void SetupFind(IEnumerable<Member> results)
    {
        var cursor = CreateCursor(results);
        _mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Member>>(),
                It.IsAny<FindOptions<Member, Member>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenFound_ReturnsMember()
    {
        var member = new Member { Username = "alice", SlackId = "U1" };
        SetupFind([member]);

        var result = await _repository.GetByUsernameAsync("alice");

        result.ShouldNotBeNull();
        result!.Username.ShouldBe("alice");
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenNotFound_ReturnsNull()
    {
        SetupFind([]);

        var result = await _repository.GetByUsernameAsync("nobody");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetBySlackIdAsync_WhenFound_ReturnsMember()
    {
        var member = new Member { Username = "bob", SlackId = "U2" };
        SetupFind([member]);

        var result = await _repository.GetBySlackIdAsync("U2");

        result.ShouldNotBeNull();
        result!.SlackId.ShouldBe("U2");
    }

    [Fact]
    public async Task GetBySlackIdAsync_WhenNotFound_ReturnsNull()
    {
        SetupFind([]);

        var result = await _repository.GetBySlackIdAsync("U_UNKNOWN");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetBySlackIdsAsync_ReturnsMatchingMembers()
    {
        var members = new List<Member>
        {
            new() { Username = "alice", SlackId = "U1" },
            new() { Username = "bob", SlackId = "U2" }
        };
        SetupFind(members);

        var result = await _repository.GetBySlackIdsAsync(["U1", "U2"]);

        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task AddAsync_SingleMember_CallsInsertOne()
    {
        var member = new Member { Username = "carol", SlackId = "U3" };
        _mockCollection
            .Setup(c => c.InsertOneAsync(member, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _repository.AddAsync(member);

        _mockCollection.Verify(
            c => c.InsertOneAsync(member, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact]
    public async Task AddAsync_MultipleMembers_CallsInsertMany()
    {
        var members = new List<Member>
        {
            new() { Username = "alice" },
            new() { Username = "bob" }
        };
        _mockCollection
            .Setup(c => c.InsertManyAsync(members, It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _repository.AddAsync(members);

        _mockCollection.Verify(
            c => c.InsertManyAsync(members, It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact]
    public async Task UpdateInfoAsync_CallsUpdateOne()
    {
        _mockCollection
            .Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Member>>(),
                It.IsAny<UpdateDefinition<Member>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

        await _repository.UpdateInfoAsync("alice", "Alice Smith", Gender.Female, SassLevel.Moderate, "Likes tea");

        _mockCollection.Verify(
            c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Member>>(),
                It.IsAny<UpdateDefinition<Member>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once());
    }
}
