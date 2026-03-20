using Bottomly.Models;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;

namespace Bottomly.Commands;

public abstract record AddKarmaResult;
public record AddKarmaSuccessResult(Karma Karma) : AddKarmaResult;
public record AddKarmaSelfAwardResult : AddKarmaResult;
public record AddKarmaErrorResult(string Error) : AddKarmaResult;

public class AddKarmaCommand(IKarmaRepository karmaRepository, ILogger<AddKarmaCommand> logger) : ICommand
{
    public string GetPurpose() => "Awards an imaginary internet point to someone/something.";

    public async Task<AddKarmaResult> ExecuteAsync(string awardedTo, string awardedBy, string reason, KarmaType karmaType)
    {
        if (karmaType == KarmaType.PozzyPoz && awardedBy.Equals(awardedTo, StringComparison.OrdinalIgnoreCase))
        {
            return new AddKarmaSelfAwardResult();
        }

        try
        {
            var karma = new Karma
            {
                AwardedToUsername = awardedTo,
                AwardedByUsername = awardedBy,
                Reason = reason,
                Awarded = DateTime.UtcNow,
                KarmaType = karmaType
            };

            await karmaRepository.AddAsync(karma);
            return new AddKarmaSuccessResult(karma);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding karma for {AwardedTo}", awardedTo);
            return new AddKarmaErrorResult(ex.Message);
        }
    }
}