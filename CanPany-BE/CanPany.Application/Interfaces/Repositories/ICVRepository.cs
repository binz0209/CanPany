using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Repositories;

public interface ICVRepository
{
    Task<CV?> GetByIdAsync(string id);
    Task<IEnumerable<CV>> GetByCandidateIdAsync(string candidateId);
    Task<CV?> GetPrimaryCVAsync(string candidateId);
    Task<CV> InsertAsync(CV entity);
    Task<bool> UpdateAsync(CV entity);
    Task<bool> DeleteAsync(string id);
}

