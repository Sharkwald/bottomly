using MongoDB.Bson.Serialization.Attributes;

namespace Bottomly.Models;

public class Member
{
    [BsonId] [BsonElement("_id")] public string Username { get; set; } = string.Empty;

    [BsonElement("slack_id")] public string SlackId { get; set; } = string.Empty;

    [BsonElement("full_name")] public string FullName { get; set; } = string.Empty;

    [BsonElement("gender")] public Gender Gender { get; set; } = Gender.Unknown;

    [BsonElement("sass_level")] public SassLevel SassLevel { get; set; } = SassLevel.Moderate;

    [BsonElement("misc_info")] public string MiscInfo { get; set; } = string.Empty;

    /// <summary>
    ///     Legacy element to enable support from pymongo persisted data
    /// </summary>
    [BsonElement("_cls")]
    public string Cls { get; set; } = string.Empty;

    [BsonIgnore]
    public string Note =>
        $"FullName: {FullName}\n" +
        $"Gender: {Gender}\n" +
        $"SassLevel: {SassLevel}\n" +
        $"MiscInfo: {MiscInfo}";
}