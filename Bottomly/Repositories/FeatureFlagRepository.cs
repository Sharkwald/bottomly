using Bottomly.Models;
using MongoDB.Driver;

namespace Bottomly.Repositories;

public class FeatureFlagRepository(IMongoDatabase database) : IFeatureFlagRepository
{
    private readonly IMongoCollection<FeatureFlag> _collection =
        database.GetCollection<FeatureFlag>("feature_flags");

    public async Task<bool> GetAsync(string flagId)
    {
        var flag = await _collection.Find(f => f.Id == flagId).FirstOrDefaultAsync();
        return flag?.Enabled ?? false;
    }

    public async Task SetAsync(string flagId, bool enabled)
    {
        var filter = Builders<FeatureFlag>.Filter.Eq(f => f.Id, flagId);
        var update = Builders<FeatureFlag>.Update.Set(f => f.Enabled, enabled);
        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task SeedAsync(string flagId, bool defaultValue)
    {
        var filter = Builders<FeatureFlag>.Filter.Eq(f => f.Id, flagId);
        var update = Builders<FeatureFlag>.Update.SetOnInsert(f => f.Enabled, defaultValue);
        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }
}