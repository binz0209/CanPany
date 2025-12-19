using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;

namespace CanPany.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repo;

    public CompanyService(ICompanyRepository repo)
    {
        _repo = repo;
    }

    public Task<Company?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);
    public Task<Company?> GetByUserIdAsync(string userId) => _repo.GetByUserIdAsync(userId);
    public Task<IEnumerable<Company>> GetAllAsync() => _repo.GetAllAsync();

    public async Task<Company> CreateAsync(Company entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.VerificationStatus = "Pending";
        entity.IsVerified = false;
        return await _repo.InsertAsync(entity);
    }

    public async Task<bool> UpdateAsync(string id, Company entity)
    {
        entity.Id = id;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(string id) => _repo.DeleteAsync(id);

    public async Task<Company?> RequestVerificationAsync(string companyId, List<string> documentUrls)
    {
        var company = await _repo.GetByIdAsync(companyId);
        if (company == null) return null;

        company.VerificationStatus = "Pending";
        company.VerificationDocuments = documentUrls;
        company.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(company);
        return company;
    }

    public async Task<Company?> ApproveVerificationAsync(string companyId, string adminId)
    {
        var company = await _repo.GetByIdAsync(companyId);
        if (company == null) return null;

        company.VerificationStatus = "Approved";
        company.IsVerified = true;
        company.VerifiedAt = DateTime.UtcNow;
        company.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(company);
        return company;
    }

    public async Task<Company?> RejectVerificationAsync(string companyId, string adminId, string reason)
    {
        var company = await _repo.GetByIdAsync(companyId);
        if (company == null) return null;

        company.VerificationStatus = "Rejected";
        company.IsVerified = false;
        company.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(company);
        return company;
    }

    public async Task<IEnumerable<Company>> GetPendingVerificationsAsync()
        => await _repo.GetByVerificationStatusAsync("Pending");
}

