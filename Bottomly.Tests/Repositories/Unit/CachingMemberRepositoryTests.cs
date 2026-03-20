using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Repositories.Unit;

public class CachingMemberRepositoryTests
{
    private readonly IMemoryCache _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
    private readonly Mock<IMemberRepository> _mockInner = new();
    private readonly CachingMemberRepository _repo;

    public CachingMemberRepositoryTests() => _repo = new CachingMemberRepository(_mockInner.Object, _cache);

    [Fact]
    public async Task GetBySlackIdAsync_CacheMiss_CallsInnerAndCachesResult()
    {
        var member = new Member { SlackId = "U1", Username = "alice" };
        _mockInner.Setup(r => r.GetBySlackIdAsync("U1")).ReturnsAsync(member);

        var result = await _repo.GetBySlackIdAsync("U1");

        result.ShouldBe(member);
        _mockInner.Verify(r => r.GetBySlackIdAsync("U1"), Times.Once);
    }

    [Fact]
    public async Task GetBySlackIdAsync_CacheHit_DoesNotCallInner()
    {
        var member = new Member { SlackId = "U1", Username = "alice" };
        _mockInner.Setup(r => r.GetBySlackIdAsync("U1")).ReturnsAsync(member);

        await _repo.GetBySlackIdAsync("U1"); // Populates cache
        await _repo.GetBySlackIdAsync("U1"); // Should hit cache

        _mockInner.Verify(r => r.GetBySlackIdAsync("U1"), Times.Once);
    }

    [Fact]
    public async Task GetByUsernameAsync_CacheMiss_CallsInnerAndCachesResult()
    {
        var member = new Member { SlackId = "U1", Username = "alice" };
        _mockInner.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(member);

        var result = await _repo.GetByUsernameAsync("alice");

        result.ShouldBe(member);
        _mockInner.Verify(r => r.GetByUsernameAsync("alice"), Times.Once);
    }

    [Fact]
    public async Task GetByUsernameAsync_CacheHit_DoesNotCallInner()
    {
        var member = new Member { SlackId = "U1", Username = "alice" };
        _mockInner.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(member);

        await _repo.GetByUsernameAsync("alice"); // Populates cache
        await _repo.GetByUsernameAsync("alice"); // Should hit cache

        _mockInner.Verify(r => r.GetByUsernameAsync("alice"), Times.Once);
    }

    [Fact]
    public async Task GetBySlackIdAsync_AfterGetByUsernameAsync_DoesNotCallInner()
    {
        var member = new Member { SlackId = "U1", Username = "alice" };
        _mockInner.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(member);

        await _repo.GetByUsernameAsync("alice"); // Caches by both keys
        await _repo.GetBySlackIdAsync("U1"); // Should hit cache (cross-key)

        _mockInner.Verify(r => r.GetBySlackIdAsync("U1"), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_PopulatesBothCacheKeys()
    {
        var members = new List<Member>
        {
            new() { SlackId = "U1", Username = "alice" },
            new() { SlackId = "U2", Username = "bob" }
        };
        _mockInner.Setup(r => r.GetAllAsync()).ReturnsAsync(members);

        var result = await _repo.GetAllAsync();

        result.ShouldBe(members);
        // Subsequent lookups should hit cache
        await _repo.GetBySlackIdAsync("U1");
        await _repo.GetByUsernameAsync("bob");
        _mockInner.Verify(r => r.GetBySlackIdAsync(It.IsAny<string>()), Times.Never);
        _mockInner.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetBySlackIdsAsync_AllCached_DoesNotCallInner()
    {
        var members = new List<Member>
        {
            new() { SlackId = "U1", Username = "alice" },
            new() { SlackId = "U2", Username = "bob" }
        };
        _mockInner.Setup(r => r.GetAllAsync()).ReturnsAsync(members);
        await _repo.GetAllAsync(); // Warm the cache

        var result = await _repo.GetBySlackIdsAsync(["U1", "U2"]);

        result.Count.ShouldBe(2);
        _mockInner.Verify(r => r.GetBySlackIdsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task GetBySlackIdsAsync_PartialCacheMiss_FetchesMissesFromInner()
    {
        var cachedMember = new Member { SlackId = "U1", Username = "alice" };
        var missedMember = new Member { SlackId = "U2", Username = "bob" };
        _mockInner.Setup(r => r.GetBySlackIdAsync("U1")).ReturnsAsync(cachedMember);
        _mockInner.Setup(r => r.GetBySlackIdsAsync(It.Is<IEnumerable<string>>(ids => ids.Contains("U2"))))
            .ReturnsAsync([missedMember]);

        await _repo.GetBySlackIdAsync("U1"); // Cache U1 only

        var result = await _repo.GetBySlackIdsAsync(["U1", "U2"]);

        result.Count.ShouldBe(2);
        _mockInner.Verify(r => r.GetBySlackIdsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_SingleMember_CachesMember()
    {
        var member = new Member { SlackId = "U1", Username = "alice" };
        _mockInner.Setup(r => r.AddAsync(member)).Returns(Task.CompletedTask);

        await _repo.AddAsync(member);
        await _repo.GetBySlackIdAsync("U1");

        _mockInner.Verify(r => r.GetBySlackIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateInfoAsync_UpdatesCacheWithFreshData()
    {
        var updated = new Member { SlackId = "U1", Username = "alice", FullName = "Alice Smith" };
        _mockInner.Setup(r =>
                r.UpdateInfoAsync("alice", "Alice Smith", It.IsAny<Gender>(), It.IsAny<SassLevel>(),
                    It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockInner.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(updated);

        await _repo.UpdateInfoAsync("alice", "Alice Smith", Gender.Unknown, SassLevel.Moderate, string.Empty);
        var result = await _repo.GetByUsernameAsync("alice");

        result!.FullName.ShouldBe("Alice Smith");
        _mockInner.Verify(r => r.GetByUsernameAsync("alice"), Times.Once); // Called once during UpdateInfoAsync
    }
}