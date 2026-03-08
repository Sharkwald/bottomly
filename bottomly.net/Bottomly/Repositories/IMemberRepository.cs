using Bottomly.Models;

namespace Bottomly.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByUsernameAsync(string username);
    Task<Member?> GetBySlackIdAsync(string slackId);
    Task<List<Member>> GetBySlackIdsAsync(IEnumerable<string> slackIds);
    Task AddAsync(Member member);
    Task AddAsync(IEnumerable<Member> members);
}