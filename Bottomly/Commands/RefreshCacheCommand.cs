using Bottomly.Repositories;
using Microsoft.Extensions.Logging;

namespace Bottomly.Commands;

public abstract record RefreshCacheResult;
public record RefreshCacheSuccessResult(int MemberCount) : RefreshCacheResult;
public record RefreshCacheErrorResult(string Error) : RefreshCacheResult;

public class RefreshCacheCommand(IMemberRepository repository, ILogger<RefreshCacheCommand> logger) : ICommand
{
    public string GetPurpose() => "Invalidates and rehydrates the member cache from the database.";

    public virtual async Task<RefreshCacheResult> ExecuteAsync()
    {
        try
        {
            logger.LogInformation("Invalidating member cache...");
            await repository.InvalidateCacheAsync();
            var members = await repository.GetAllAsync();
            logger.LogInformation("Member cache rehydrated with {Count} members.", members.Count);
            return new RefreshCacheSuccessResult(members.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing member cache.");
            return new RefreshCacheErrorResult(ex.Message);
        }
    }
}
