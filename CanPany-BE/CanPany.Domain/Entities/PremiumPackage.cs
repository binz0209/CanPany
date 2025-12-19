// CanPany.Domain/Entities/PremiumPackage.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class PremiumPackage
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("userType")]
    public string UserType { get; set; } = "Candidate"; // Candidate/Company

    [BsonElement("packageType")]
    public string PackageType { get; set; } = string.Empty; // AIPremium/JobPosting/AIScreening

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending/Active/Expired/Cancelled

    [BsonElement("purchasedAt")]
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("activatedAt")]
    public DateTime? ActivatedAt { get; set; }

    [BsonElement("paymentMethod")]
    public string PaymentMethod { get; set; } = "Banking"; // Banking/SePay

    [BsonElement("transactionId")]
    public string? TransactionId { get; set; }

    [BsonElement("adminNotes")]
    public string? AdminNotes { get; set; }
}

