using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bottomly.Repositories;

public class MemberCachePopulator(IMemberRepository repository, ILogger<MemberCachePopulator> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Warming member cache...");
        var members = await repository.GetAllAsync();
        logger.LogInformation("Member cache warmed with {Count} members.", members.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}