using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetByUserId(string userId, [FromQuery] int limit = 100)
        => Ok(await _auditService.GetLogsByUserIdAsync(userId, limit));

    [HttpGet("by-entity/{entityType}")]
    public async Task<IActionResult> GetByEntity(string entityType, [FromQuery] string? entityId = null, [FromQuery] int limit = 100)
        => Ok(await _auditService.GetLogsByEntityAsync(entityType, entityId, limit));

    [HttpGet("errors")]
    public async Task<IActionResult> GetErrors([FromQuery] int limit = 100)
        => Ok(await _auditService.GetErrorLogsAsync(limit));

    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int limit = 1000)
        => Ok(await _auditService.GetLogsByDateRangeAsync(startDate, endDate, limit));
}

