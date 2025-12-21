using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

public interface ICVService
{
    Task<CV?> GetByIdAsync(string id);
    Task<IEnumerable<CV>> GetByCandidateIdAsync(string candidateId);
    Task<CV?> GetPrimaryCVAsync(string candidateId);
    Task<CV> CreateAsync(CV cv);
    Task<bool> UpdateAsync(string id, CV cv);
    Task<bool> DeleteAsync(string id);
    Task<bool> SetPrimaryAsync(string cvId, string candidateId);
    Task<CV> AnalyzeCVAsync(string cvId, string? jobId = null);
    Task<CV> GenerateCVForJobAsync(string candidateId, string jobId, string baseCVId);
}




