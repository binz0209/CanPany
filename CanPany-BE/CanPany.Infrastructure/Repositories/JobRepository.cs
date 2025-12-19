using CanPany.Application.Interfaces.Repositories;
using CanPany.Domain.Entities;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly IMongoCollection<Job> _collection;

    public JobRepository(IMongoCollection<Job> collection)
    {
        _collection = collection;
    }

    public async Task<Job?> GetByIdAsync(string id)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<Job>> GetAllAsync()
        => await _collection.Find(_ => true).SortByDescending(x => x.CreatedAt).ToListAsync();

    public async Task<IEnumerable<Job>> GetByCompanyIdAsync(string companyId)
        => await _collection.Find(x => x.CompanyId == companyId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Job>> GetByStatusAsync(string status)
        => await _collection.Find(x => x.Status == status)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Job>> GetOpenJobsAsync()
        => await _collection.Find(x => x.Status == "Open")
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<Job> InsertAsync(Job entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> UpdateAsync(Job entity)
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

    public async Task<Job?> UpdateStatusAsync(string id, string newStatus)
    {
        var update = Builders<Job>.Update
            .Set(j => j.Status, newStatus)
            .Set(j => j.UpdatedAt, DateTime.UtcNow);
        
        var result = await _collection.FindOneAndUpdateAsync(
            j => j.Id == id,
            update,
            new FindOneAndUpdateOptions<Job> { ReturnDocument = ReturnDocument.After }
        );
        return result;
    }
}

