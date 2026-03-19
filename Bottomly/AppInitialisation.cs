using Bottomly.Repositories;
using Bottomly.Seed;
using Bottomly.Slack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bottomly;

public static class AppInitialisation
{
    public static async Task InitialiseAsync(this IHost app)
    {
        var featureFlagRepository = app.Services.GetRequiredService<IFeatureFlagRepository>();
        await featureFlagRepository.SeedAsync("EnableLlm", false);

        var populator = app.Services.GetRequiredService<MemberlistPopulator>();
        await populator.PopulateMembers();

        if (app.Services.GetRequiredService<IConfiguration>().GetValue<bool>("ImportMemberSeedData"))
        {
            var importer = app.Services.GetRequiredService<MemberSeedDataImporter>();
            await importer.ImportAsync();
        }
    }
}
