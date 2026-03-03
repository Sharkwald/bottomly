using Bottomly.Models;
using MongoDB.Driver;

namespace Bottomly.Repositories;

public class MemberRepository(IMongoDatabase database) : IMemberRepository
{
    private readonly IMongoCollection<Member> _collection = database.GetCollection<Member>("member");

    public async Task<Member?> GetByUsernameAsync(string username)
    {
        var result = await _collection.Find(m => m.Username == username).FirstOrDefaultAsync();
        return result;
    }

    public async Task<Member?> GetBySlackIdAsync(string slackId)
    {
        var result = await _collection.Find(m => m.SlackId == slackId).FirstOrDefaultAsync();
        return result;
    }

    public async Task AddAsync(Member member) => await _collection.InsertOneAsync(member);
    public async Task AddAsync(IEnumerable<Member> members) => await _collection.InsertManyAsync(members);
}