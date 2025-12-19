using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

public interface IGeminiService
{
    /// <summary>
    /// Extract skills từ text (bio, description, etc.) sử dụng Gemini
    /// </summary>
    Task<List<string>> ExtractSkillsAsync(string text);

    /// <summary>
    /// Generate embedding vector từ list of skills sử dụng Gemini
    /// </summary>
    Task<List<double>> GenerateEmbeddingAsync(List<string> skills);

    /// <summary>
    /// Analyze CV and calculate ATS score
    /// </summary>
    Task<CVAnalysis> AnalyzeCVAsync(string cvText, List<string> jobKeywords);

    /// <summary>
    /// Extract skills from CV text
    /// </summary>
    Task<ExtractedSkills> ExtractSkillsFromCVAsync(string cvText);

    /// <summary>
    /// Rewrite CV section in STAR format
    /// </summary>
    Task<string> GenerateSTARRewriteAsync(string sectionText, string sectionType);

    /// <summary>
    /// Generate job description using AI
    /// </summary>
    Task<string> GenerateJobDescriptionAsync(string title, List<string> requiredSkills, string jobType, string? location);

    /// <summary>
    /// Chat with AI assistant
    /// </summary>
    Task<string> ChatAsync(string message, string? context);
}

