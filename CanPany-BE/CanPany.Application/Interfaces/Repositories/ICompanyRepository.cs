using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(string id);
    Task<Company?> GetByUserIdAsync(string userId);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company> InsertAsync(Company entity);
    Task<bool> UpdateAsync(Company entity);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<Company>> GetByVerificationStatusAsync(string status);
}

