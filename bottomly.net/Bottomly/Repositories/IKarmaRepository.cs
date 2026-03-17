using Bottomly.Models;

namespace Bottomly.Repositories;

public interface IKarmaRepository
{
    Task AddAsync(Karma karma);
    Task<int> GetCurrentNetKarmaAsync(string recipient);
    Task<KarmaReasonsResult> GetKarmaReasonsAsync(string recipient);
    Task<IReadOnlyList<KarmaScore>> GetLeaderBoardAsync(int size = 3);
    Task<IReadOnlyList<KarmaScore>> GetLoserBoardAsync(int size = 3);
}

public record KarmaScore(string Username, int NetKarma);

public record KarmaReasonsResult(int Reasonless, IReadOnlyList<Karma> Reasoned);