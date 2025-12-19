using CanPany.Application.Interfaces.Repositories;
using CanPany.Domain.Entities;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IMongoCollection<AuditLog> _collection;

    public AuditLogRepository(IMongoCollection<AuditLog> collection)
    {
        _collection = collection;
    }

    public async Task<AuditLog> InsertAsync(AuditLog entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int limit = 100)
        => await _collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType, string? entityId = null, int limit = 100)
    {
        var filter = Builders<AuditLog>.Filter.Eq(x => x.EntityType, entityType);
        if (!string.IsNullOrEmpty(entityId))
        {
            filter = Builders<AuditLog>.Filter.And(
                filter,
                Builders<AuditLog>.Filter.Eq(x => x.EntityId, entityId)
            );
        }
        return await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEndpointAsync(string endpoint, int limit = 100)
        => await _collection.Find(x => x.Endpoint == endpoint)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 1000)
        => await _collection.Find(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, int limit = 100)
        => await _collection.Find(x => x.Action == action)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetErrorLogsAsync(int limit = 100)
        => await _collection.Find(x => x.ErrorMessage != null)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync();

    public async Task<long> CountAsync()
        => await _collection.CountDocumentsAsync(_ => true);

    public async Task<bool> DeleteOldLogsAsync(DateTime beforeDate)
    {
        var result = await _collection.DeleteManyAsync(x => x.CreatedAt < beforeDate);
        return result.DeletedCount > 0;
    }
}

