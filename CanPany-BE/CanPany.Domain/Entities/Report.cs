// CanPany.Domain/Entities/Report.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class Report
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("reporterId"), BsonRepresentation(BsonType.ObjectId)]
    public string ReporterId { get; set; } = null!;

    [BsonElement("reportType")]
    public string ReportType { get; set; } = string.Empty; // Job/Application/User/Company

    [BsonElement("targetId")]
    public string TargetId { get; set; } = null!; // ID of reported item

    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty; // Spam/Fraud/Inappropriate/etc.

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending/Reviewed/Resolved/Dismissed

    [BsonElement("adminNotes")]
    public string? AdminNotes { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    [BsonElement("resolvedBy")]
    public string? ResolvedBy { get; set; } // Admin user ID
}

