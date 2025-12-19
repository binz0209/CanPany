using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;

namespace CanPany.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repo;

    public AuditService(IAuditLogRepository repo)
    {
        _repo = repo;
    }

    public async Task LogAsync(AuditLog auditLog)
    {
        auditLog.CreatedAt = DateTime.UtcNow;
        await _repo.InsertAsync(auditLog);
    }

    public async Task LogRequestAsync(
        string? userId,
        string? userEmail,
        string httpMethod,
        string endpoint,
        string requestPath,
        string? queryString = null,
        string? requestBody = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = httpMethod,
            HttpMethod = httpMethod,
            Endpoint = endpoint,
            RequestPath = requestPath,
            QueryString = queryString,
            RequestBody = requestBody,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await LogAsync(auditLog);
    }

    public async Task LogResponseAsync(
        string? userId,
        string endpoint,
        int statusCode,
        string? responseBody = null,
        long? duration = null)
    {
        // Log response as a separate entry (will be linked by endpoint and timestamp in queries)
        var auditLog = new AuditLog
        {
            UserId = userId,
            Endpoint = endpoint,
            Action = "RESPONSE",
            ResponseStatusCode = statusCode,
            ResponseBody = responseBody,
            Duration = duration
        };
        await LogAsync(auditLog);
    }

    public async Task LogErrorAsync(
        string? userId,
        string endpoint,
        string errorMessage,
        string? stackTrace = null,
        int? statusCode = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Endpoint = endpoint,
            Action = "ERROR",
            ErrorMessage = errorMessage,
            StackTrace = stackTrace,
            ResponseStatusCode = statusCode
        };

        await LogAsync(auditLog);
    }

    public async Task LogEntityChangeAsync(
        string? userId,
        string action,
        string entityType,
        string? entityId,
        Dictionary<string, object>? changes = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Changes = changes
        };

        await LogAsync(auditLog);
    }

    public Task<IEnumerable<AuditLog>> GetLogsByUserIdAsync(string userId, int limit = 100)
        => _repo.GetByUserIdAsync(userId, limit);

    public Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityType, string? entityId = null, int limit = 100)
        => _repo.GetByEntityTypeAsync(entityType, entityId, limit);

    public Task<IEnumerable<AuditLog>> GetErrorLogsAsync(int limit = 100)
        => _repo.GetErrorLogsAsync(limit);

    public Task<IEnumerable<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 1000)
        => _repo.GetByDateRangeAsync(startDate, endDate, limit);
}

