using CanPany.Application.Interfaces.Repositories;
using CanPany.Domain.Entities;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly IMongoCollection<Company> _collection;

    public CompanyRepository(IMongoCollection<Company> collection)
    {
        _collection = collection;
    }

    public async Task<Company?> GetByIdAsync(string id)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Company?> GetByUserIdAsync(string userId)
        => await _collection.Find(x => x.UserId == userId).FirstOrDefaultAsync();

    public async Task<IEnumerable<Company>> GetAllAsync()
        => await _collection.Find(_ => true).ToListAsync();

    public async Task<Company> InsertAsync(Company entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> UpdateAsync(Company entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var result = await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<IEnumerable<Company>> GetByVerificationStatusAsync(string status)
        => await _collection.Find(x => x.VerificationStatus == status).ToListAsync();
}

