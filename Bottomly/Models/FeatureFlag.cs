using MongoDB.Bson.Serialization.Attributes;

namespace Bottomly.Models;

public class FeatureFlag
{
    [BsonId][BsonElement("_id")] public string Id { get; set; } = string.Empty;

    [BsonElement("enabled")] public bool Enabled { get; set; }
}
