using Bottomly.Slack;
using Microsoft.Extensions.DependencyInjection;

namespace Bottomly.Seed;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBottomlySeeding(this IServiceCollection services)
    {
        services.AddSingleton<MemberlistPopulator>();
        services.AddSingleton<MemberSeedDataImporter>();

        return services;
    }
}