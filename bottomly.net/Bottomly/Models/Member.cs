using MongoDB.Bson.Serialization.Attributes;

namespace Bottomly.Models;

public class Member
{
    [BsonId] [BsonElement("_id")] public string Username { get; set; } = string.Empty;

    [BsonElement("slack_id")] public string SlackId { get; set; } = string.Empty;
}