using Bottomly.Models;
using MongoDB.Driver;

namespace Bottomly.Repositories;

public class MemberRepository(IMongoDatabase database) : IMemberRepository
{
    private readonly IMongoCollection<Member> _collection = database.GetCollection<Member>("member");

    public Task<List<Member>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public async Task<Member?> GetByUsernameAsync(string username) =>
        await _collection.Find(m => m.Username == username).FirstOrDefaultAsync();

    public async Task<Member?> GetBySlackIdAsync(string slackId) =>
        await _collection.Find(m => m.SlackId == slackId).FirstOrDefaultAsync();

    public Task<List<Member>> GetBySlackIdsAsync(IEnumerable<string> slackIds) =>
        _collection.Find(m => slackIds.Contains(m.SlackId)).ToListAsync();

    public async Task AddAsync(Member member) => await _collection.InsertOneAsync(member);
    public async Task AddAsync(IEnumerable<Member> members) => await _collection.InsertManyAsync(members);

    public async Task UpdateInfoAsync(string username, string fullName, Gender gender, SassLevel sassLevel,
        string miscInfo)
    {
        var update = Builders<Member>.Update
            .Set(m => m.FullName, fullName)
            .Set(m => m.Gender, gender)
            .Set(m => m.SassLevel, sassLevel)
            .Set(m => m.MiscInfo, miscInfo);
        await _collection.UpdateOneAsync(m => m.Username == username, update);
    }
}