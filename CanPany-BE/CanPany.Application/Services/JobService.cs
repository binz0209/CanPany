using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;

namespace CanPany.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _repo;
    private readonly IUserProfileService _userProfileService;
    private readonly ISkillService _skillService;
    private readonly IGeminiService _geminiService;

    public JobService(
        IJobRepository repo,
        IUserProfileService userProfileService,
        ISkillService skillService,
        IGeminiService geminiService)
    {
        _repo = repo;
        _userProfileService = userProfileService;
        _skillService = skillService;
        _geminiService = geminiService;
    }

    public Task<Job?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);
    public Task<IEnumerable<Job>> GetAllAsync() => _repo.GetAllAsync();
    public Task<IEnumerable<Job>> GetByCompanyIdAsync(string companyId) => _repo.GetByCompanyIdAsync(companyId);
    public Task<IEnumerable<Job>> GetOpenJobsAsync() => _repo.GetOpenJobsAsync();

    public async Task<Job> CreateAsync(Job entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Status = "Open";
        return await _repo.InsertAsync(entity);
    }

    public async Task<bool> UpdateAsync(string id, Job entity)
    {
        entity.Id = id;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(string id) => _repo.DeleteAsync(id);
    public Task<Job?> UpdateStatusAsync(string id, string newStatus) => _repo.UpdateStatusAsync(id, newStatus);

    public async Task<IEnumerable<(Job Job, double Similarity)>> GetRecommendedJobsAsync(string candidateId, int limit = 10)
    {
        // Similar logic to ProjectService.GetRecommendedProjectsAsync
        var userProfile = await _userProfileService.GetByUserIdAsync(candidateId);
        if (userProfile == null)
            return Enumerable.Empty<(Job, double)>();

        var userSkillIds = new HashSet<string>();
        if (userProfile.SkillIds != null && userProfile.SkillIds.Count > 0)
        {
            userSkillIds = userProfile.SkillIds.ToHashSet();
        }

        if (userSkillIds.Count == 0)
            return Enumerable.Empty<(Job, double)>();

        var openJobs = await _repo.GetOpenJobsAsync();
        var jobsWithSimilarity = new List<(Job Job, double Similarity)>();

        foreach (var job in openJobs)
        {
            var jobSkillIds = new HashSet<string>();
            if (job.RequiredSkills != null && job.RequiredSkills.Count > 0)
            {
                jobSkillIds = job.RequiredSkills.ToHashSet();
            }

            if (jobSkillIds.Count == 0)
                continue;

            int matchedSkills = userSkillIds.Intersect(jobSkillIds).Count();
            int totalJobSkills = jobSkillIds.Count;

            if (matchedSkills == 0)
                continue;

            double similarity = (double)matchedSkills / totalJobSkills * 100.0;
            similarity = Math.Min(100.0, similarity);

            jobsWithSimilarity.Add((job, similarity));
        }

        return jobsWithSimilarity
            .OrderByDescending(x => x.Similarity)
            .Take(limit);
    }

    public async Task<string> GenerateJobDescriptionAsync(string title, List<string> requiredSkills, string jobType)
    {
        // Use Gemini to generate job description
        var skillsText = string.Join(", ", requiredSkills);
        var prompt = $"Generate a professional job description for a {jobType} position: {title}. Required skills: {skillsText}. Include job responsibilities, requirements, and benefits.";
        
        // This will be implemented in GeminiService
        // For now, return placeholder
        return await Task.FromResult($"Job description for {title}");
    }
}

