// CanPany.Domain/Entities/Notification.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class Notification
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // ProposalAccepted, NewMessage...
    [BsonElement("title")]
    public string? Title { get; set; } 

    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("payload")]
    public string? Payload { get; set; } // JSON nhẹ

    [BsonElement("isRead")]
    public bool IsRead { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
