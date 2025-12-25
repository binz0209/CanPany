using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;

namespace CanPany.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _repo;

    public ApplicationService(IApplicationRepository repo)
    {
        _repo = repo;
    }

    public Task<JobApplication?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);

    public Task<IEnumerable<JobApplication>> GetByJobIdAsync(string jobId) => _repo.GetByJobIdAsync(jobId);

    public Task<IEnumerable<JobApplication>> GetByCandidateIdAsync(string candidateId) => _repo.GetByCandidateIdAsync(candidateId);

    public async Task<JobApplication> CreateAsync(JobApplication entity)
    {
        entity.AppliedAt = DateTime.UtcNow;
        entity.Status = entity.Status ?? "Pending";
        return await _repo.InsertAsync(entity);
    }

    public async Task<bool> UpdateStatusAsync(string id, string newStatus)
    {
        var updated = await _repo.UpdateStatusAsync(id, newStatus);
        return updated != null;
    }

    public Task<bool> DeleteAsync(string id) => _repo.DeleteAsync(id);
}

