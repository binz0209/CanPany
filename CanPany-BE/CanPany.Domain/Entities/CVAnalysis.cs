// CanPany.Domain/Entities/CVAnalysis.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

public class CVAnalysis
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("cvId"), BsonRepresentation(BsonType.ObjectId)]
    public string CVId { get; set; } = null!;

    [BsonElement("candidateId"), BsonRepresentation(BsonType.ObjectId)]
    public string CandidateId { get; set; } = null!;

    [BsonElement("atsScore")]
    public double ATSScore { get; set; } // 0-100

    [BsonElement("scoreBreakdown")]
    public ATSScoreBreakdown ScoreBreakdown { get; set; } = new();

    [BsonElement("extractedSkills")]
    public ExtractedSkills ExtractedSkills { get; set; } = new();

    [BsonElement("missingKeywords")]
    public List<string> MissingKeywords { get; set; } = new();

    [BsonElement("improvementSuggestions")]
    public List<string> ImprovementSuggestions { get; set; } = new();

    [BsonElement("starRewrittenSections")]
    public Dictionary<string, string> STARRewrittenSections { get; set; } = new(); // Section -> STAR format

    [BsonElement("analyzedAt")]
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class ATSScoreBreakdown
{
    [BsonElement("keywords")]
    public double Keywords { get; set; } // 0-100

    [BsonElement("formatting")]
    public double Formatting { get; set; } // 0-100

    [BsonElement("skills")]
    public double Skills { get; set; } // 0-100

    [BsonElement("experience")]
    public double Experience { get; set; } // 0-100

    [BsonElement("education")]
    public double Education { get; set; } // 0-100
}

public class ExtractedSkills
{
    [BsonElement("technical")]
    public List<string> Technical { get; set; } = new();

    [BsonElement("soft")]
    public List<string> Soft { get; set; } = new();
}

