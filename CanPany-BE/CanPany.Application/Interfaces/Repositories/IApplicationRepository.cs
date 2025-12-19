using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Repositories;

public interface IApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(string id);
    Task<IEnumerable<JobApplication>> GetByJobIdAsync(string jobId);
    Task<IEnumerable<JobApplication>> GetByCandidateIdAsync(string candidateId);
    Task<JobApplication> InsertAsync(JobApplication entity);
    Task<bool> UpdateAsync(JobApplication entity);
    Task<bool> DeleteAsync(string id);
    Task<JobApplication?> UpdateStatusAsync(string id, string newStatus);
}

