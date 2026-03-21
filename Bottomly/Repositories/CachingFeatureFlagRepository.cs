using Microsoft.Extensions.Caching.Memory;

namespace Bottomly.Repositories;

public class CachingFeatureFlagRepository(IFeatureFlagRepository inner, IMemoryCache cache) : IFeatureFlagRepository
{
    private const string KeyPrefix = "feature_flag:";

    public Task<bool> GetAsync(string flagId) =>
        cache.GetOrCreateAsync(KeyPrefix + flagId, _ => inner.GetAsync(flagId));

    public async Task SetAsync(string flagId, bool enabled)
    {
        await inner.SetAsync(flagId, enabled);
        cache.Set(KeyPrefix + flagId, enabled);
    }

    public async Task SeedAsync(string flagId, bool defaultValue)
    {
        await inner.SeedAsync(flagId, defaultValue);
        cache.Remove(KeyPrefix + flagId);
    }
}