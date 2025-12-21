using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _svc;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly IUserSettingsService _userSettingsService;

    public ProjectsController(
        IProjectService svc,
        INotificationService notificationService,
        IUserService userService,
        IUserSettingsService userSettingsService)
    {
        _svc = svc;
        _notificationService = notificationService;
        _userService = userService;
        _userSettingsService = userSettingsService;
    }

    [AllowAnonymous]
    [HttpGet("{id}")] public async Task<IActionResult> GetById(string id) => Ok(await _svc.GetByIdAsync(id));

    [AllowAnonymous]
    [HttpGet("open")] public async Task<IActionResult> Open() => Ok(await _svc.GetOpenProjectsAsync());

    [Authorize]
    [HttpGet("by-owner/{ownerId}")]
    public async Task<IActionResult> ByOwner(string ownerId) => Ok(await _svc.GetByOwnerIdAsync(ownerId));

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Project dto)
    {
        try
        {
            if (dto == null)
                return BadRequest(new { message = "Project data is required" });

            var created = await _svc.CreateAsync(dto);
            
            // 🔔 Gửi notification "dự án mới" cho tất cả users (trừ owner)
            // Wrap trong try-catch riêng để không ảnh hưởng đến việc tạo project
            try
            {
            Console.WriteLine($"📩 [ProjectsController.Create] Starting notification creation for project: {created.Id}");
            
            // Lấy danh sách tất cả users
            var allUsers = await _userService.GetAllAsync();
            
            var ownerId = created.OwnerId;
            var owner = await _userService.GetByIdAsync(ownerId);
            var ownerName = owner?.FullName ?? "Người dùng";
            
            Console.WriteLine($"📩 [ProjectsController.Create] Found {allUsers.Count()} users. Owner: {ownerName}");
            
            foreach (var user in allUsers)
            {
                // Bỏ qua owner
                if (user.Id == ownerId)
                    continue;

                // Kiểm tra settings
                var userSettings = await _userSettingsService.GetByUserIdAsync(user.Id);
                if (userSettings?.NotificationSettings?.NewProjectNotifications == false)
                {
                    Console.WriteLine($"⚠️ [ProjectsController.Create] User {user.Id} has new project notifications disabled. Skipping.");
                    continue;
                }

                var payload = JsonSerializer.Serialize(new
                {
                    projectId = created.Id,
                    projectTitle = created.Title,
                    ownerId = ownerId,
                    ownerName = ownerName,
                    action = "NewProject"
                });

                var notification = new Notification
                {
                    UserId = user.Id,
                    Type = "NewProject",
                    Title = "Dự án mới",
                    Message = $"{ownerName} đã đăng dự án mới: {created.Title}",
                    Payload = payload,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.CreateAsync(notification);
                Console.WriteLine($"✅ [ProjectsController.Create] Notification sent to user {user.Id}");
            }
            
            Console.WriteLine($"✅ [ProjectsController.Create] Notifications sent successfully");
        }
        catch (Exception ex)
        {
                Console.WriteLine($"❌ [ProjectsController.Create] Failed to send project notifications: {ex.Message}");
                Console.WriteLine($"❌ [ProjectsController.Create] Stack trace: {ex.StackTrace}");
            }

            return Ok(created);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [ProjectsController.Create] Error creating project: {ex.Message}");
            return StatusCode(500, new { message = "Failed to create project", error = ex.Message });
        }
    }

    [Authorize] // chỉ cần đăng nhập
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Project dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Project ID is required" });
            if (dto == null)
                return BadRequest(new { message = "Project data is required" });

            // lấy userId từ JWT
            var currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            // lấy project hiện tại
            var existing = await _svc.GetByIdAsync(id);
            if (existing is null) return NotFound(new { message = "Project not found" });

            // chỉ owner mới được sửa
            if (!string.Equals(existing.OwnerId, currentUserId, StringComparison.Ordinal))
                return Forbid(); // 403

            // gán lại Id để chắc chắn update đúng bản ghi
            dto.Id = id;
            // (tuỳ bạn: có thể chặn đổi OwnerId, CreatedAt, v.v.)
            dto.OwnerId = existing.OwnerId;
            dto.CreatedAt = existing.CreatedAt;

            var updated = await _svc.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [ProjectsController.Update] Error updating project: {ex.Message}");
            return StatusCode(500, new { message = "Failed to update project", error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(string id) => Ok(await _svc.DeleteAsync(id));

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [AllowAnonymous]
    [HttpGet("status/{status}")]
    public async Task<IActionResult> ByStatus(string status)
        => Ok(await _svc.GetByStatusAsync(status));

    [Authorize]
    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommended([FromQuery] int limit = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var recommendations = await _svc.GetRecommendedProjectsAsync(userId, limit);
        
        // Format response với similarity score
        // Lưu ý: r.Similarity đã là percentage (0-100) rồi, không cần nhân 100 nữa
        var result = recommendations.Select(r => new
        {
            project = r.Project,
            similarity = Math.Round(Math.Min(100.0, r.Similarity), 2) // Đảm bảo max 100%
        });

        return Ok(result);
    }
}
