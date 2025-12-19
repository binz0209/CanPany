using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(AuditLog auditLog);
    Task LogRequestAsync(
        string? userId,
        string? userEmail,
        string httpMethod,
        string endpoint,
        string requestPath,
        string? queryString = null,
        string? requestBody = null,
        string? ipAddress = null,
        string? userAgent = null);
    
    Task LogResponseAsync(
        string? userId,
        string endpoint,
        int statusCode,
        string? responseBody = null,
        long? duration = null);
    
    Task LogErrorAsync(
        string? userId,
        string endpoint,
        string errorMessage,
        string? stackTrace = null,
        int? statusCode = null);
    
    Task LogEntityChangeAsync(
        string? userId,
        string action,
        string entityType,
        string? entityId,
        Dictionary<string, object>? changes = null);
    
    Task<IEnumerable<AuditLog>> GetLogsByUserIdAsync(string userId, int limit = 100);
    Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityType, string? entityId = null, int limit = 100);
    Task<IEnumerable<AuditLog>> GetErrorLogsAsync(int limit = 100);
    Task<IEnumerable<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 1000);
}

