namespace Bottomly.Repositories;

public interface IFeatureFlagRepository
{
    Task<bool> GetAsync(string flagId);
    Task SetAsync(string flagId, bool enabled);
    Task SeedAsync(string flagId, bool defaultValue);
}