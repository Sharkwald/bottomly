using Bottomly.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Bottomly.Repositories;

public class KarmaRepository(IMongoDatabase database) : IKarmaRepository
{
    private readonly IMongoCollection<Karma> _collection = database.GetCollection<Karma>("karma");

    public async Task AddAsync(Karma karma) => await _collection.InsertOneAsync(karma);

    public async Task<int> GetCurrentNetKarmaAsync(string recipient)
    {
        var scores = await GetNetKarmaAggregateAsync(recipient.ToLower(), 1);
        return scores.FirstOrDefault()?.NetKarma ?? 0;
    }

    public async Task<KarmaReasonsResult> GetKarmaReasonsAsync(string recipient)
    {
        var cutOff = CutOffDate();
        var lower = recipient.ToLower();

        var pipeline = new[]
        {
            new BsonDocument("$project", new BsonDocument
            {
                { "awarded_to_username", new BsonDocument("$toLower", "$awarded_to_username") },
                { "awarded_by_username", new BsonDocument("$toLower", "$awarded_by_username") },
                { "karma_type", "$karma_type" },
                { "awarded", "$awarded" },
                { "reason", "$reason" }
            }),
            new BsonDocument("$match", new BsonDocument
            {
                { "awarded_to_username", lower },
                { "awarded", new BsonDocument("$gt", cutOff) }
            })
        };

        var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

        var karmaList = results.Select(r => new Karma
        {
            AwardedToUsername = r["awarded_to_username"].AsString,
            AwardedByUsername = r["awarded_by_username"].AsString,
            KarmaTypeValue = r["karma_type"].AsString,
            Awarded = r["awarded"].ToUniversalTime(),
            Reason = r.Contains("reason") ? r["reason"].AsString : string.Empty
        }).ToList();

        var reasoned = karmaList.Where(k => !string.IsNullOrEmpty(k.Reason)).ToList();
        var reasonless = karmaList.Count(k => string.IsNullOrEmpty(k.Reason));

        return new KarmaReasonsResult(reasonless, reasoned);
    }

    public async Task<IReadOnlyList<KarmaScore>> GetLeaderBoardAsync(int size = 3) =>
        await GetNetKarmaAggregateAsync(limit: size, ascending: false);

    public async Task<IReadOnlyList<KarmaScore>> GetLoserBoardAsync(int size = 3) =>
        await GetNetKarmaAggregateAsync(limit: size, ascending: true);

    private async Task<IReadOnlyList<KarmaScore>> GetNetKarmaAggregateAsync(
        string? recipient = null, int limit = 3, bool ascending = false)
    {
        var cutOff = CutOffDate();
        var sortDirection = ascending ? 1 : -1;

        var matchFilter = new BsonDocument("awarded", new BsonDocument("$gt", cutOff));
        if (recipient != null)
        {
            matchFilter["recipient"] = recipient;
        }

        var pipeline = new List<BsonDocument>
        {
            new("$project", new BsonDocument
            {
                { "recipient", new BsonDocument("$toLower", "$awarded_to_username") },
                {
                    "net_karma", new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$karma_type", nameof(KarmaType.PozzyPoz) }),
                        1, -1
                    })
                },
                { "awarded", "$awarded" }
            }),
            new("$match", matchFilter),
            new("$group", new BsonDocument
            {
                { "_id", "$recipient" },
                { "net_karma", new BsonDocument("$sum", "$net_karma") }
            }),
            new("$sort", new BsonDocument("net_karma", sortDirection))
        };

        var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

        return results.Take(limit)
            .Select(r => new KarmaScore(r["_id"].AsString, r["net_karma"].AsInt32))
            .ToList();
    }

    private static DateTime CutOffDate() => DateTime.UtcNow.AddDays(-Karma.ExpiryDays);
}