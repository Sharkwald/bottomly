using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Bottomly.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBottomlyRepositories(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IKarmaRepository, KarmaRepository>();
        services.AddSingleton<IMemberRepository>(sp =>
            new CachingMemberRepository(
                new MemberRepository(sp.GetRequiredService<IMongoDatabase>()),
                sp.GetRequiredService<IMemoryCache>()));
        services.AddSingleton<IFeatureFlagRepository>(sp =>
            new CachingFeatureFlagRepository(
                new FeatureFlagRepository(sp.GetRequiredService<IMongoDatabase>()),
                sp.GetRequiredService<IMemoryCache>()));

        return services;
    }
}