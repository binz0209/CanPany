// CanPany.Domain/Entities/ExternalSync.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class ExternalSync
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("platform")]
    public string Platform { get; set; } = string.Empty; // LinkedIn/GitHub

    [BsonElement("accessToken")]
    public string? AccessToken { get; set; }

    [BsonElement("refreshToken")]
    public string? RefreshToken { get; set; }

    [BsonElement("tokenExpiresAt")]
    public DateTime? TokenExpiresAt { get; set; }

    [BsonElement("lastSyncedAt")]
    public DateTime? LastSyncedAt { get; set; }

    [BsonElement("syncStatus")]
    public string SyncStatus { get; set; } = "Connected"; // Connected/Disconnected/Error

    [BsonElement("syncedData")]
    public Dictionary<string, object>? SyncedData { get; set; } // JSON data from platform

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

