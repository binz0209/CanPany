using CanPany.Application.Interfaces.Repositories;
using CanPany.Domain.Entities;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly IMongoCollection<JobApplication> _collection;

    public ApplicationRepository(IMongoCollection<JobApplication> collection)
    {
        _collection = collection;
    }

    public async Task<JobApplication?> GetByIdAsync(string id)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<JobApplication>> GetByJobIdAsync(string jobId)
        => await _collection.Find(x => x.JobId == jobId)
            .SortByDescending(x => x.AppliedAt)
            .ToListAsync();

    public async Task<IEnumerable<JobApplication>> GetByCandidateIdAsync(string candidateId)
        => await _collection.Find(x => x.CandidateId == candidateId)
            .SortByDescending(x => x.AppliedAt)
            .ToListAsync();

    public async Task<JobApplication> InsertAsync(JobApplication entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> UpdateAsync(JobApplication entity)
    {
        var result = await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<JobApplication?> UpdateStatusAsync(string id, string newStatus)
    {
        var update = Builders<JobApplication>.Update
            .Set(a => a.Status, newStatus)
            .Set(a => a.ReviewedAt, DateTime.UtcNow);
        
        var result = await _collection.FindOneAndUpdateAsync(
            a => a.Id == id,
            update,
            new FindOneAndUpdateOptions<JobApplication> { ReturnDocument = ReturnDocument.After }
        );
        return result;
    }
}

