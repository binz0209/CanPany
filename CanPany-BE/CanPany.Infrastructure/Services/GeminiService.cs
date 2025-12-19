using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace CanPany.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GeminiService(IConfiguration configuration, HttpClient httpClient)
    {
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey not configured");
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
    }

    public async Task<List<string>> ExtractSkillsAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        try
        {
            var prompt = $@"Phân tích đoạn text sau và trích xuất các kỹ năng (skills) liên quan đến công nghệ, lập trình, thiết kế, marketing, v.v.
Chỉ trả về danh sách các skills, mỗi skill trên một dòng, không có số thứ tự, không có giải thích.

Text:
{text}

Trả về chỉ danh sách skills, mỗi skill một dòng:";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"models/gemini-1.5-flash:generateContent?key={_apiKey}", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var result = responseObj
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            // Parse skills từ response
            var skills = result
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Where(s => !s.StartsWith("#") && !s.StartsWith("-") && !s.StartsWith("*"))
                .Select(s => s.TrimStart('-', '*', ' ', '#').Trim())
                .Where(s => s.Length > 0)
                .Distinct()
                .ToList();

            return skills;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting skills: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<List<double>> GenerateEmbeddingAsync(List<string> skills)
    {
        if (skills == null || skills.Count == 0)
            return new List<double>(new double[768]);

        try
        {
            // Combine skills thành một text
            var skillsText = string.Join(", ", skills);

            var prompt = $@"Tạo embedding vector cho các kỹ năng sau. 
Trả về một mảng JSON chứa 768 số thực (double) đại diện cho vector embedding.

Skills: {skillsText}

Trả về chỉ mảng JSON với 768 phần tử, ví dụ: [0.1, 0.2, 0.3, ...]";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"models/gemini-1.5-flash:generateContent?key={_apiKey}", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var result = responseObj
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            // Parse JSON array
            // Loại bỏ markdown code blocks nếu có
            if (result.StartsWith("```"))
            {
                var lines = result.Split('\n');
                result = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
            }

            result = result.Trim();

            // Parse JSON
            var embedding = JsonSerializer.Deserialize<List<double>>(result);
            
            if (embedding == null || embedding.Count == 0)
            {
                // Fallback: tạo embedding giả (zero vector)
                return new List<double>(new double[768]);
            }

            // Normalize về 768 dimensions nếu cần
            if (embedding.Count < 768)
            {
                var normalized = new List<double>(embedding);
                while (normalized.Count < 768)
                {
                    normalized.Add(0.0);
                }
                return normalized;
            }
            else if (embedding.Count > 768)
            {
                return embedding.Take(768).ToList();
            }

            return embedding;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating embedding: {ex.Message}");
            // Fallback: return zero vector
            return new List<double>(new double[768]);
        }
    }

    public async Task<CVAnalysis> AnalyzeCVAsync(string cvText, List<string> jobKeywords)
    {
        try
        {
            var keywordsText = string.Join(", ", jobKeywords);
            var prompt = $@"Phân tích CV sau và đánh giá ATS score (0-100) dựa trên các tiêu chí:
1. Keywords matching (0-30 điểm): So sánh với keywords: {keywordsText}
2. Formatting (0-20 điểm): Cấu trúc, font, spacing
3. Skills (0-20 điểm): Kỹ năng được liệt kê rõ ràng
4. Experience (0-20 điểm): Kinh nghiệm được mô tả chi tiết
5. Education (0-10 điểm): Học vấn được trình bày đầy đủ

CV Text:
{cvText}

Trả về JSON với format:
{{
  ""atsScore"": 85,
  ""scoreBreakdown"": {{
    ""keywords"": 25,
    ""formatting"": 18,
    ""skills"": 17,
    ""experience"": 18,
    ""education"": 9
  }},
  ""missingKeywords"": [""keyword1"", ""keyword2""],
  ""improvementSuggestions"": [""suggestion1"", ""suggestion2""]
}}";

            var result = await CallGeminiAsync(prompt);
            var analysis = ParseCVAnalysis(result);
            return analysis;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing CV: {ex.Message}");
            return new CVAnalysis
            {
                ATSScore = 0,
                ScoreBreakdown = new ATSScoreBreakdown(),
                MissingKeywords = new List<string>(),
                ImprovementSuggestions = new List<string> { "Error analyzing CV" }
            };
        }
    }

    public async Task<ExtractedSkills> ExtractSkillsFromCVAsync(string cvText)
    {
        try
        {
            var prompt = $@"Trích xuất kỹ năng từ CV sau. Phân loại thành Technical Skills và Soft Skills.
Trả về JSON format:
{{
  ""technical"": [""skill1"", ""skill2""],
  ""soft"": [""skill1"", ""skill2""]
}}

CV Text:
{cvText}";

            var result = await CallGeminiAsync(prompt);
            return ParseExtractedSkills(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting skills from CV: {ex.Message}");
            return new ExtractedSkills();
        }
    }

    public async Task<string> GenerateSTARRewriteAsync(string sectionText, string sectionType)
    {
        try
        {
            var prompt = $@"Viết lại phần {sectionType} sau theo định dạng STAR (Situation-Task-Action-Result):
- Situation: Tình huống/ngữ cảnh
- Task: Nhiệm vụ cần thực hiện
- Action: Hành động đã thực hiện
- Result: Kết quả đạt được

Nội dung gốc:
{sectionText}

Trả về chỉ phần viết lại theo STAR format, không có giải thích thêm.";

            var result = await CallGeminiAsync(prompt);
            return result.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating STAR rewrite: {ex.Message}");
            return sectionText; // Return original if error
        }
    }

    public async Task<string> GenerateJobDescriptionAsync(string title, List<string> requiredSkills, string jobType, string? location)
    {
        try
        {
            var skillsText = string.Join(", ", requiredSkills);
            var locationText = location ?? "Remote/Hybrid";
            
            var prompt = $@"Tạo mô tả công việc chuyên nghiệp cho vị trí {title} ({jobType}) tại {locationText}.

Required Skills: {skillsText}

Bao gồm:
- Job Overview
- Key Responsibilities
- Required Qualifications
- Preferred Qualifications
- Benefits & Perks

Trả về mô tả công việc hoàn chỉnh, chuyên nghiệp.";

            var result = await CallGeminiAsync(prompt);
            return result.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating job description: {ex.Message}");
            return $"Job description for {title}";
        }
    }

    public async Task<string> ChatAsync(string message, string? context)
    {
        try
        {
            var contextText = context != null ? $"\nContext: {context}" : "";
            var prompt = $@"Bạn là trợ lý AI chuyên về tuyển dụng và nghề nghiệp. Trả lời câu hỏi một cách hữu ích và chuyên nghiệp.{contextText}

Câu hỏi: {message}

Trả lời:";

            var result = await CallGeminiAsync(prompt);
            return result.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in chat: {ex.Message}");
            return "Xin lỗi, tôi không thể trả lời câu hỏi này lúc này.";
        }
    }

    private async Task<string> CallGeminiAsync(string prompt)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"models/gemini-1.5-flash:generateContent?key={_apiKey}", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        var result = responseObj
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";

        // Remove markdown code blocks if present
        if (result.StartsWith("```"))
        {
            var lines = result.Split('\n');
            result = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
        }

        return result.Trim();
    }

    private CVAnalysis ParseCVAnalysis(string jsonText)
    {
        try
        {
            var json = jsonText.Trim();
            if (json.StartsWith("```json"))
            {
                json = json.Substring(7);
            }
            if (json.EndsWith("```"))
            {
                json = json.Substring(0, json.Length - 3);
            }
            json = json.Trim();

            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            var analysis = new CVAnalysis
            {
                ATSScore = doc.GetProperty("atsScore").GetDouble(),
                ScoreBreakdown = new ATSScoreBreakdown
                {
                    Keywords = doc.GetProperty("scoreBreakdown").GetProperty("keywords").GetDouble(),
                    Formatting = doc.GetProperty("scoreBreakdown").GetProperty("formatting").GetDouble(),
                    Skills = doc.GetProperty("scoreBreakdown").GetProperty("skills").GetDouble(),
                    Experience = doc.GetProperty("scoreBreakdown").GetProperty("experience").GetDouble(),
                    Education = doc.GetProperty("scoreBreakdown").GetProperty("education").GetDouble()
                },
                MissingKeywords = doc.GetProperty("missingKeywords").EnumerateArray().Select(e => e.GetString()!).ToList(),
                ImprovementSuggestions = doc.GetProperty("improvementSuggestions").EnumerateArray().Select(e => e.GetString()!).ToList()
            };
            return analysis;
        }
        catch
        {
            return new CVAnalysis { ATSScore = 0, ScoreBreakdown = new ATSScoreBreakdown() };
        }
    }

    private ExtractedSkills ParseExtractedSkills(string jsonText)
    {
        try
        {
            var json = jsonText.Trim();
            if (json.StartsWith("```json"))
            {
                json = json.Substring(7);
            }
            if (json.EndsWith("```"))
            {
                json = json.Substring(0, json.Length - 3);
            }
            json = json.Trim();

            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            return new ExtractedSkills
            {
                Technical = doc.GetProperty("technical").EnumerateArray().Select(e => e.GetString()!).ToList(),
                Soft = doc.GetProperty("soft").EnumerateArray().Select(e => e.GetString()!).ToList()
            };
        }
        catch
        {
            return new ExtractedSkills();
        }
    }
}
