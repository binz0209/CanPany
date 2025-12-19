// CanPany.Domain/Entities/Company.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class Company
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!; // Reference to User.Id

    [BsonElement("companyName")]
    public string CompanyName { get; set; } = string.Empty;

    [BsonElement("logoUrl")]
    public string? LogoUrl { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("website")]
    public string? Website { get; set; }

    [BsonElement("industry")]
    public string? Industry { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("companySize")]
    public string CompanySize { get; set; } = "Small"; // Small/Medium/Large

    [BsonElement("verificationStatus")]
    public string VerificationStatus { get; set; } = "Pending"; // Pending/Approved/Rejected

    [BsonElement("verificationDocuments")]
    public List<string> VerificationDocuments { get; set; } = new(); // URLs to business documents

    [BsonElement("isVerified")]
    public bool IsVerified { get; set; } = false;

    [BsonElement("verifiedAt")]
    public DateTime? VerifiedAt { get; set; }

    [BsonElement("premiumFeatures")]
    public PremiumFeatures PremiumFeatures { get; set; } = new();

    [BsonElement("premiumExpiresAt")]
    public DateTime? PremiumExpiresAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class PremiumFeatures
{
    [BsonElement("jobPosting")]
    public bool JobPosting { get; set; } = false;

    [BsonElement("aiScreening")]
    public bool AIScreening { get; set; } = false;

    [BsonElement("jobPostingCredits")]
    public int JobPostingCredits { get; set; } = 0;

    [BsonElement("aiScreeningCredits")]
    public int AIScreeningCredits { get; set; } = 0;
}

