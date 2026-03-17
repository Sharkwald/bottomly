using Bottomly.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;

namespace Bottomly.Commands;

public class ReleaseCommand(IOptions<BottomlyOptions> options, ILogger<ReleaseCommand> logger)
    : ICommand
{
    private readonly string _token = options.Value.GitHubToken;

    public string GetPurpose() => "Describes the latest release of bottomly";

    public virtual async Task<string?> ExecuteAsync()
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue("bottomly"))
            {
                Credentials = new Credentials(_token)
            };

            var user = await client.User.Current();
            var release = await client.Repository.Release.GetLatest(user.Login, "bottomly");

            var desc = $"Latest Release: *{release.Name}* v{release.TagName}";
            desc += $"\n_Published at {release.PublishedAt}_";
            if (!string.IsNullOrEmpty(release.Body))
            {
                desc += $"\n{release.Body}";
            }

            return desc;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving release info from Github");
            return null;
        }
    }
}