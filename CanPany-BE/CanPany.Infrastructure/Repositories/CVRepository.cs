using CanPany.Application.Interfaces.Repositories;
using CanPany.Domain.Entities;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class CVRepository : ICVRepository
{
    private readonly IMongoCollection<CV> _collection;

    public CVRepository(IMongoCollection<CV> collection)
    {
        _collection = collection;
    }

    public async Task<CV?> GetByIdAsync(string id)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<CV>> GetByCandidateIdAsync(string candidateId)
        => await _collection.Find(x => x.CandidateId == candidateId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<CV?> GetPrimaryCVAsync(string candidateId)
        => await _collection.Find(x => x.CandidateId == candidateId && x.IsPrimary == true)
            .FirstOrDefaultAsync();

    public async Task<CV> InsertAsync(CV entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> UpdateAsync(CV entity)
    {
        var result = await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }
}

