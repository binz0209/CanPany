using System.Security.Claims;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _svc;
    private readonly IUserSettingsService _settingsSvc;
    
    public UsersController(IUserService svc, IUserSettingsService settingsSvc)
    {
        _svc = svc;
        _settingsSvc = settingsSvc;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        try
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id)) return Unauthorized();
            
            var me = await _svc.GetByIdAsync(id);
            if (me == null) return NotFound(new { message = "User not found" });
            
            return Ok(me);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve user information", error = ex.Message });
        }
    }
    
    [Authorize]
    //[Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "User ID is required" });
            
            var user = await _svc.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found" });
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve user", error = ex.Message });
        }
    }

    public record UpdateUserDto(string? FullName, string? Role, string? AvatarUrl);
    
    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
    {
        try
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id)) return Unauthorized();
            
            var u = await _svc.GetByIdAsync(id);
            if (u is null) return NotFound(new { message = "User not found" });
            
            if (!string.IsNullOrEmpty(dto.FullName)) u.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.AvatarUrl)) u.AvatarUrl = dto.AvatarUrl;
        
            return Ok(await _svc.UpdateAsync(id, u));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update user", error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "User ID is required" });
            if (dto == null)
                return BadRequest(new { message = "User data is required" });

            var u = await _svc.GetByIdAsync(id);
            if (u is null) return NotFound(new { message = "User not found" });
            
            if (!string.IsNullOrEmpty(dto.FullName)) u.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.Role)) u.Role = dto.Role;
            if (!string.IsNullOrEmpty(dto.AvatarUrl)) u.AvatarUrl = dto.AvatarUrl;
            
            return Ok(await _svc.UpdateAsync(id, u));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update user", error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "User ID is required" });

            var deleted = await _svc.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to delete user", error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _svc.GetAllAsync();
            // chỉ trả về thông tin cơ bản để chat
            var list = users.Select(u => new { u.Id, u.FullName, u.Email }).ToList();
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve users", error = ex.Message });
        }
    }


    public record ChangePasswordRequest(string OldPassword, string NewPassword);

    public record UpdateNotificationSettingsRequest(bool EmailNotifications, bool MessageNotifications, bool NewProjectNotifications);

    public record UpdatePrivacySettingsRequest(bool PublicProfile, bool ShowOnlineStatus);

    public record UpdateUserSettingsRequest(
        NotificationSettingsDto? NotificationSettings,
        PrivacySettingsDto? PrivacySettings
    );

    public record NotificationSettingsDto(bool EmailNotifications, bool MessageNotifications, bool NewProjectNotifications);

    public record PrivacySettingsDto(bool PublicProfile, bool ShowOnlineStatus);

    [Authorize]
    [HttpGet("me/settings")]
    public async Task<IActionResult> GetUserSettings()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var settings = await _settingsSvc.EnsureAsync(userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve user settings", error = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("me/settings")]
    public async Task<IActionResult> UpdateUserSettings([FromBody] UpdateUserSettingsRequest dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (dto == null)
                return BadRequest(new { message = "Settings data is required" });

            var settings = await _settingsSvc.EnsureAsync(userId);

            // Update notification settings if provided
            if (dto.NotificationSettings != null)
            {
                settings.NotificationSettings = new NotificationSettings
                {
                    EmailNotifications = dto.NotificationSettings.EmailNotifications,
                    MessageNotifications = dto.NotificationSettings.MessageNotifications,
                    NewProjectNotifications = dto.NotificationSettings.NewProjectNotifications
                };
            }

            // Update privacy settings if provided
            if (dto.PrivacySettings != null)
            {
                settings.PrivacySettings = new PrivacySettings
                {
                    PublicProfile = dto.PrivacySettings.PublicProfile,
                    ShowOnlineStatus = dto.PrivacySettings.ShowOnlineStatus
                };
            }

            await _settingsSvc.UpdateAsync(settings.Id, settings);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update user settings", error = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("me/notification-settings")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsRequest dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (dto == null)
                return BadRequest(new { message = "Notification settings data is required" });

            var notificationSettings = new NotificationSettings
            {
                EmailNotifications = dto.EmailNotifications,
                MessageNotifications = dto.MessageNotifications,
                NewProjectNotifications = dto.NewProjectNotifications
            };

            var settings = await _settingsSvc.UpdateNotificationSettingsAsync(userId, notificationSettings);
            return Ok(settings.NotificationSettings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update notification settings", error = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("me/privacy-settings")]
    public async Task<IActionResult> UpdatePrivacySettings([FromBody] UpdatePrivacySettingsRequest dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (dto == null)
                return BadRequest(new { message = "Privacy settings data is required" });

            var privacySettings = new PrivacySettings
            {
                PublicProfile = dto.PublicProfile,
                ShowOnlineStatus = dto.ShowOnlineStatus
            };

            var settings = await _settingsSvc.UpdatePrivacySettingsAsync(userId, privacySettings);
            return Ok(settings.PrivacySettings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update privacy settings", error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();
            if (req == null)
                return BadRequest(new { message = "Password change request is required" });

            var result = await _svc.ChangePasswordAsync(userId, req.OldPassword, req.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { message = "Password change failed", errors = result.Errors });

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to change password", error = ex.Message });
        }
    }
}
