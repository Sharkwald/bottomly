using Bottomly.Models;
using Bottomly.Repositories;
using Bottomly.Tests.Infrastructure;
using MongoDB.Driver;
using Shouldly;

namespace Bottomly.Tests.Repositories.Integration;

[Collection("MongoDB")]
public class MemberRepositoryIntegrationTests(MongoDbFixture fixture) : IAsyncLifetime
{
    private IMongoDatabase _db = null!;
    private MemberRepository _sut = null!;

    public Task InitializeAsync()
    {
        _db = fixture.GetDatabase($"member_test_{Guid.NewGuid():N}");
        _sut = new MemberRepository(_db);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() =>
        await fixture.Client.DropDatabaseAsync(_db.DatabaseNamespace.DatabaseName);

    [Fact]
    public async Task GetByUsernameAsync_WhenMemberExists_ReturnsMember()
    {
        await _sut.AddAsync(new Member { Username = "alice", SlackId = "U001" });

        var result = await _sut.GetByUsernameAsync("alice");

        result.ShouldNotBeNull();
        result.Username.ShouldBe("alice");
        result.SlackId.ShouldBe("U001");
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenMemberDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetByUsernameAsync("nobody");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetBySlackIdAsync_WhenMemberExists_ReturnsMember()
    {
        await _sut.AddAsync(new Member { Username = "bob", SlackId = "U002" });

        var result = await _sut.GetBySlackIdAsync("U002");

        result.ShouldNotBeNull();
        result.SlackId.ShouldBe("U002");
        result.Username.ShouldBe("bob");
    }

    [Fact]
    public async Task GetBySlackIdAsync_WhenMemberDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetBySlackIdAsync("UNOBODY");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetBySlackIdsAsync_ReturnsOnlyMatchingMembers()
    {
        await _sut.AddAsync([
            new Member { Username = "alice", SlackId = "U001" },
            new Member { Username = "bob", SlackId = "U002" },
            new Member { Username = "carol", SlackId = "U003" }
        ]);

        var result = await _sut.GetBySlackIdsAsync(["U001", "U003"]);

        result.Count.ShouldBe(2);
        result.Select(m => m.Username).ShouldBe(["alice", "carol"], true);
    }

    [Fact]
    public async Task AddAsync_SingleMember_PersistsMemberWithAllFields()
    {
        var member = new Member
        {
            Username = "dave",
            SlackId = "U004",
            FullName = "Dave Smith",
            Gender = Gender.Male,
            SassLevel = SassLevel.Frequent,
            MiscInfo = "Loves coffee"
        };

        await _sut.AddAsync(member);

        var stored = await _sut.GetByUsernameAsync("dave");
        stored.ShouldNotBeNull();
        stored.FullName.ShouldBe("Dave Smith");
        stored.Gender.ShouldBe(Gender.Male);
        stored.SassLevel.ShouldBe(SassLevel.Frequent);
        stored.MiscInfo.ShouldBe("Loves coffee");
    }

    [Fact]
    public async Task AddAsync_BatchOfMembers_PersistsAllMembers()
    {
        var members = new List<Member>
        {
            new() { Username = "eve", SlackId = "U005" },
            new() { Username = "frank", SlackId = "U006" },
            new() { Username = "grace", SlackId = "U007" }
        };

        await _sut.AddAsync(members);

        var eve = await _sut.GetByUsernameAsync("eve");
        var frank = await _sut.GetByUsernameAsync("frank");
        var grace = await _sut.GetByUsernameAsync("grace");
        eve.ShouldNotBeNull();
        frank.ShouldNotBeNull();
        grace.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateInfoAsync_UpdatesOnlyInfoFields_LeavesSlackIdUntouched()
    {
        await _sut.AddAsync(new Member
        {
            Username = "heidi",
            SlackId = "U008",
            FullName = "Heidi Old",
            Gender = Gender.Unknown,
            SassLevel = SassLevel.None,
            MiscInfo = "Old info"
        });

        await _sut.UpdateInfoAsync("heidi", "Heidi New", Gender.Female, SassLevel.Constant, "New info");

        var updated = await _sut.GetByUsernameAsync("heidi");
        updated.ShouldNotBeNull();
        updated.FullName.ShouldBe("Heidi New");
        updated.Gender.ShouldBe(Gender.Female);
        updated.SassLevel.ShouldBe(SassLevel.Constant);
        updated.MiscInfo.ShouldBe("New info");
        updated.SlackId.ShouldBe("U008");
    }

    [Fact]
    public async Task UpdateInfoAsync_WhenUsernameDoesNotExist_DoesNotThrow() =>
        // Should silently no-op (UpdateOne with no match)
        await Should.NotThrowAsync(() =>
            _sut.UpdateInfoAsync("ghost", "Ghost", Gender.Unknown, SassLevel.None, ""));
}