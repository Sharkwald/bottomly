using Bottomly.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Bottomly.Repositories;

public class CachingMemberRepository(IMemberRepository inner, IMemoryCache cache) : IMemberRepository
{
    private const string SlackKeyPrefix = "member:slack:";
    private const string UsernameKeyPrefix = "member:username:";
    private volatile CancellationTokenSource _evictionToken = new();

    public async Task<List<Member>> GetAllAsync()
    {
        var members = await inner.GetAllAsync();
        foreach (var member in members)
        {
            CacheMember(member);
        }

        return members;
    }

    public async Task<Member?> GetByUsernameAsync(string username)
    {
        if (cache.TryGetValue(UsernameKeyPrefix + username, out Member? cached))
        {
            return cached;
        }

        var member = await inner.GetByUsernameAsync(username);
        if (member is not null)
        {
            CacheMember(member);
        }

        return member;
    }

    public async Task<Member?> GetBySlackIdAsync(string slackId)
    {
        if (cache.TryGetValue(SlackKeyPrefix + slackId, out Member? cached))
        {
            return cached;
        }

        var member = await inner.GetBySlackIdAsync(slackId);
        if (member is not null)
        {
            CacheMember(member);
        }

        return member;
    }

    public async Task<List<Member>> GetBySlackIdsAsync(IEnumerable<string> slackIds)
    {
        var idList = slackIds.ToList();
        var result = new List<Member>();
        var misses = new List<string>();

        foreach (var id in idList)
        {
            if (cache.TryGetValue(SlackKeyPrefix + id, out Member? cached))
            {
                result.Add(cached!);
            }
            else
            {
                misses.Add(id);
            }
        }

        if (misses.Count <= 0)
        {
            return result;
        }

        var fetched = await inner.GetBySlackIdsAsync(misses);
        foreach (var member in fetched)
        {
            CacheMember(member);
            result.Add(member);
        }

        return result;
    }

    public async Task AddAsync(Member member)
    {
        await inner.AddAsync(member);
        CacheMember(member);
    }

    public async Task AddAsync(IEnumerable<Member> members)
    {
        var memberList = members.ToList();
        await inner.AddAsync(memberList);
        foreach (var member in memberList)
        {
            CacheMember(member);
        }
    }

    public async Task UpdateInfoAsync(string username, string fullName, Gender gender, SassLevel sassLevel,
        string miscInfo)
    {
        await inner.UpdateInfoAsync(username, fullName, gender, sassLevel, miscInfo);
        var updated = await inner.GetByUsernameAsync(username);
        if (updated is not null)
        {
            CacheMember(updated);
        }
        else
        {
            cache.Remove(UsernameKeyPrefix + username);
        }
    }

    public Task InvalidateCacheAsync()
    {
        var old = Interlocked.Exchange(ref _evictionToken, new CancellationTokenSource());
        old.Cancel();
        old.Dispose();
        return Task.CompletedTask;
    }

    private void CacheMember(Member member)
    {
        var options = new MemoryCacheEntryOptions()
            .AddExpirationToken(new CancellationChangeToken(_evictionToken.Token));
        cache.Set(SlackKeyPrefix + member.SlackId, member, options);
        cache.Set(UsernameKeyPrefix + member.Username, member, options);
    }
}