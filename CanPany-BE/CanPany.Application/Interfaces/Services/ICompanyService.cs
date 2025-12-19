using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

public interface ICompanyService
{
    Task<Company?> GetByIdAsync(string id);
    Task<Company?> GetByUserIdAsync(string userId);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company> CreateAsync(Company entity);
    Task<bool> UpdateAsync(string id, Company entity);
    Task<bool> DeleteAsync(string id);
    Task<Company?> RequestVerificationAsync(string companyId, List<string> documentUrls);
    Task<Company?> ApproveVerificationAsync(string companyId, string adminId);
    Task<Company?> RejectVerificationAsync(string companyId, string adminId, string reason);
    Task<IEnumerable<Company>> GetPendingVerificationsAsync();
}

