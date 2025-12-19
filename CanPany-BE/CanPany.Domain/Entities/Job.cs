// CanPany.Domain/Entities/Job.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class Job
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("companyId"), BsonRepresentation(BsonType.ObjectId)]
    public string CompanyId { get; set; } = null!;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("requiredSkills")]
    public List<string> RequiredSkills { get; set; } = new(); // Skill IDs

    [BsonElement("skillEmbedding")]
    public List<double>? SkillEmbedding { get; set; } // Vector embedding for semantic search

    [BsonElement("jobType")]
    public string JobType { get; set; } = "FullTime"; // FullTime/PartTime/Contract/Remote

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("salaryRange")]
    public SalaryRange? SalaryRange { get; set; }

    [BsonElement("experienceLevel")]
    public string ExperienceLevel { get; set; } = "Mid"; // Entry/Mid/Senior/Executive

    [BsonElement("status")]
    public string Status { get; set; } = "Open"; // Open/Closed/Filled

    [BsonElement("premiumBoost")]
    public bool PremiumBoost { get; set; } = false; // Visibility upgrade

    [BsonElement("images")]
    public List<string> Images { get; set; } = new(); // Reference images URLs

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class SalaryRange
{
    [BsonElement("min")]
    public decimal? Min { get; set; }

    [BsonElement("max")]
    public decimal? Max { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "VND";
}

