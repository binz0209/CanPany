// CanPany.Domain/Entities/CV.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class CV
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("candidateId"), BsonRepresentation(BsonType.ObjectId)]
    public string CandidateId { get; set; } = null!;

    [BsonElement("fileUrl")]
    public string FileUrl { get; set; } = string.Empty; // PDF/DOCX URL

    [BsonElement("fileName")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("fileSize")]
    public long FileSize { get; set; } // in bytes

    [BsonElement("fileType")]
    public string FileType { get; set; } = string.Empty; // "pdf" or "docx"

    [BsonElement("extractedText")]
    public string? ExtractedText { get; set; } // Text extracted from CV

    [BsonElement("atsScore")]
    public double? ATSScore { get; set; } // 0-100

    [BsonElement("extractedSkills")]
    public List<string> ExtractedSkills { get; set; } = new(); // Technical + soft skills

    [BsonElement("keywords")]
    public List<string> Keywords { get; set; } = new(); // Important keywords

    [BsonElement("starRewrittenContent")]
    public Dictionary<string, string>? STARRewrittenContent { get; set; } // Section -> STAR format

    [BsonElement("analysisDate")]
    public DateTime? AnalysisDate { get; set; }

    [BsonElement("isPrimary")]
    public bool IsPrimary { get; set; } = false; // Primary CV for applications

    [BsonElement("version")]
    public int Version { get; set; } = 1;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

