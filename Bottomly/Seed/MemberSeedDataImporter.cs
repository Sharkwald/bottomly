using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Bottomly.Seed;

public class MemberSeedDataImporter(
    IMemberRepository memberRepository,
    IHostEnvironment env,
    ILogger<MemberSeedDataImporter> logger)
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public async Task ImportAsync()
    {
        var seedDir = ResolveSeedDir();
        if (!Directory.Exists(seedDir))
        {
            logger.LogWarning("MemberSeedData directory not found at {Path}. Skipping seed data import.", seedDir);
            return;
        }

        var files = Directory.GetFiles(seedDir, "*.yaml");
        logger.LogInformation("Importing seed data from {Count} YAML files in {Path}", files.Length, seedDir);

        foreach (var file in files)
            await ImportFileAsync(file);

        logger.LogInformation("Seed data import complete.");
    }

    private string ResolveSeedDir()
    {
        var dir = new DirectoryInfo(env.ContentRootPath);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "MemberSeedData");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        return Path.Combine(env.ContentRootPath, "MemberSeedData");
    }

    private async Task ImportFileAsync(string file)
    {
        try
        {
            var yaml = await File.ReadAllTextAsync(file);
            var dto = _deserializer.Deserialize<MemberSeedDataDto>(yaml);

            if (!Enum.TryParse<Gender>(dto.Gender, ignoreCase: true, out var gender))
            {
                logger.LogWarning("Unknown gender value '{Value}' in {File}. Defaulting to Unknown.", dto.Gender, Path.GetFileName(file));
                gender = Gender.Unknown;
            }

            if (!Enum.TryParse<SassLevel>(dto.SassLevel, ignoreCase: true, out var sassLevel))
            {
                logger.LogWarning("Unknown sass_level value '{Value}' in {File}. Defaulting to Moderate.", dto.SassLevel, Path.GetFileName(file));
                sassLevel = SassLevel.Moderate;
            }

            var member = await memberRepository.GetByUsernameAsync(dto.Username);
            if (member is null)
            {
                logger.LogWarning("Member '{Username}' not found in DB. Skipping.", dto.Username);
                return;
            }

            await memberRepository.UpdateInfoAsync(dto.Username, dto.FullName, gender, sassLevel, dto.MiscInfo);
            logger.LogInformation("Updated member '{Username}'.", dto.Username);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import seed data from {File}.", Path.GetFileName(file));
        }
    }
}
