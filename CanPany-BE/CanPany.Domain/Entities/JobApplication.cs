// CanPany.Domain/Entities/JobApplication.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class JobApplication
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("jobId"), BsonRepresentation(BsonType.ObjectId)]
    public string JobId { get; set; } = null!;

    [BsonElement("candidateId"), BsonRepresentation(BsonType.ObjectId)]
    public string CandidateId { get; set; } = null!;

    [BsonElement("cvId"), BsonRepresentation(BsonType.ObjectId)]
    public string? CVId { get; set; } // Reference to CV

    [BsonElement("coverLetter")]
    public string CoverLetter { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending/Reviewing/Accepted/Rejected/Withdrawn

    [BsonElement("aiScore")]
    public double? AIScore { get; set; } // Matching score from AI (0-100)

    [BsonElement("appliedAt")]
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("reviewedAt")]
    public DateTime? ReviewedAt { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; } // Company notes about the application
}

