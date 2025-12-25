using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

public interface IApplicationService
{
    Task<JobApplication?> GetByIdAsync(string id);
    Task<IEnumerable<JobApplication>> GetByJobIdAsync(string jobId);
    Task<IEnumerable<JobApplication>> GetByCandidateIdAsync(string candidateId);
    Task<JobApplication> CreateAsync(JobApplication entity);
    Task<bool> UpdateStatusAsync(string id, string newStatus);
    Task<bool> DeleteAsync(string id);
}

