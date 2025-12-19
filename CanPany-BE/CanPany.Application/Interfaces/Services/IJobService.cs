using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

public interface IJobService
{
    Task<Job?> GetByIdAsync(string id);
    Task<IEnumerable<Job>> GetAllAsync();
    Task<IEnumerable<Job>> GetByCompanyIdAsync(string companyId);
    Task<IEnumerable<Job>> GetOpenJobsAsync();
    Task<Job> CreateAsync(Job entity);
    Task<bool> UpdateAsync(string id, Job entity);
    Task<bool> DeleteAsync(string id);
    Task<Job?> UpdateStatusAsync(string id, string newStatus);
    Task<IEnumerable<(Job Job, double Similarity)>> GetRecommendedJobsAsync(string candidateId, int limit = 10);
    Task<string> GenerateJobDescriptionAsync(string title, List<string> requiredSkills, string jobType);
}

