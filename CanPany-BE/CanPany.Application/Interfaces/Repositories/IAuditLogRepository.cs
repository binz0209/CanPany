using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task<AuditLog> InsertAsync(AuditLog entity);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int limit = 100);
    Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType, string? entityId = null, int limit = 100);
    Task<IEnumerable<AuditLog>> GetByEndpointAsync(string endpoint, int limit = 100);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 1000);
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action, int limit = 100);
    Task<IEnumerable<AuditLog>> GetErrorLogsAsync(int limit = 100);
    Task<long> CountAsync();
    Task<bool> DeleteOldLogsAsync(DateTime beforeDate);
}

