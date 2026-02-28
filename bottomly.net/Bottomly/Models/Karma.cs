using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bottomly.Models;

public class Karma
{
    public const int ExpiryDays = 30;

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("awarded_to_username")] public string AwardedToUsername { get; set; } = string.Empty;

    [BsonElement("awarded_by_username")] public string AwardedByUsername { get; set; } = string.Empty;

    [BsonElement("reason")] public string Reason { get; set; } = string.Empty;

    [BsonElement("awarded")] public DateTime Awarded { get; set; }

    [BsonElement("karma_type")] public string KarmaTypeValue { get; set; } = string.Empty;

    [BsonIgnore]
    public KarmaType KarmaType
    {
        get => KarmaTypeValue == nameof(KarmaType.PozzyPoz) ? KarmaType.PozzyPoz : KarmaType.NeggyNeg;
        set => KarmaTypeValue = value.ToString();
    }
}