using Bottomly.Models;

namespace Bottomly.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByUsernameAsync(string username);
    Task<Member?> GetBySlackIdAsync(string slackId);
    Task AddAsync(Member member);
}