using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Repositories;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(string id);
    Task<IEnumerable<Job>> GetAllAsync();
    Task<IEnumerable<Job>> GetByCompanyIdAsync(string companyId);
    Task<IEnumerable<Job>> GetByStatusAsync(string status);
    Task<IEnumerable<Job>> GetOpenJobsAsync();
    Task<Job> InsertAsync(Job entity);
    Task<bool> UpdateAsync(Job entity);
    Task<bool> DeleteAsync(string id);
    Task<Job?> UpdateStatusAsync(string id, string newStatus);
}

