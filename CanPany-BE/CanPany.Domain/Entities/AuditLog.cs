// CanPany.Domain/Entities/AuditLog.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class AuditLog
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; } // User who performed the action

    [BsonElement("userEmail")]
    public string? UserEmail { get; set; }

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty; // GET, POST, PUT, DELETE, etc.

    [BsonElement("entityType")]
    public string? EntityType { get; set; } // User, Job, Company, etc.

    [BsonElement("entityId")]
    public string? EntityId { get; set; } // ID of the affected entity

    [BsonElement("endpoint")]
    public string Endpoint { get; set; } = string.Empty; // API endpoint

    [BsonElement("httpMethod")]
    public string HttpMethod { get; set; } = string.Empty; // GET, POST, PUT, DELETE

    [BsonElement("requestPath")]
    public string RequestPath { get; set; } = string.Empty; // Full request path

    [BsonElement("queryString")]
    public string? QueryString { get; set; }

    [BsonElement("requestBody")]
    public string? RequestBody { get; set; } // JSON string of request body

    [BsonElement("responseStatusCode")]
    public int? ResponseStatusCode { get; set; }

    [BsonElement("responseBody")]
    public string? ResponseBody { get; set; } // JSON string of response body

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("duration")]
    public long? Duration { get; set; } // Request duration in milliseconds

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; } // Error message if any

    [BsonElement("stackTrace")]
    public string? StackTrace { get; set; } // Stack trace if error occurred

    [BsonElement("changes")]
    public Dictionary<string, object>? Changes { get; set; } // Before/After values for updates

    [BsonElement("metadata")]
    public Dictionary<string, object>? Metadata { get; set; } // Additional metadata

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

