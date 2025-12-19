using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;

namespace CanPany.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IRealtimeService _realtime;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;
    private readonly IUserSettingsService _userSettingsService;

    public NotificationService(
        INotificationRepository repo, 
        IRealtimeService realtime,
        IEmailService emailService,
        IUserService userService,
        IUserSettingsService userSettingsService)
    {
        _repo = repo;
        _realtime = realtime;
        _emailService = emailService;
        _userService = userService;
        _userSettingsService = userSettingsService;
    }

    public Task<IEnumerable<Notification>> GetByUserAsync(string userId)
        => _repo.GetByUserAsync(userId);

    public Task<Notification?> GetByIdAsync(string id)
        => _repo.GetByIdAsync(id);

    public async Task<Notification> CreateAsync(Notification entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsRead = false;
        
        Console.WriteLine($"📝 NotificationService.CreateAsync: Type={entity.Type}, UserId={entity.UserId}, Title={entity.Title}");
        
        var saved = await _repo.InsertAsync(entity);
        
        Console.WriteLine($"✅ Notification saved to DB. Id: {saved.Id}");
        
        // Send realtime notification
        try
        {
            await _realtime.SendToUserAsync(saved.UserId, saved);
            Console.WriteLine($"📡 Realtime notification sent to userId: {saved.UserId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to send realtime notification: {ex.Message}");
        }
        
        // Send email notification (async, fire and forget)
        _ = SendEmailNotificationAsync(saved);
        
        return saved;
    }

    private async Task SendEmailNotificationAsync(Notification notification)
    {
        try
        {
            var user = await _userService.GetByIdAsync(notification.UserId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return;

            // Kiểm tra email notification settings
            var userSettings = await _userSettingsService.GetByUserIdAsync(notification.UserId);
            if (userSettings?.NotificationSettings?.EmailNotifications == false)
            {
                Console.WriteLine($"⚠️ Email notifications disabled for user {notification.UserId}. Skipping email.");
                return;
            }

            var subject = $"CanPany - {notification.Title}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>{notification.Title}</h2>
                    <p style='font-size: 16px;'>{notification.Message}</p>
                    <p style='color: #666; font-size: 14px;'>Thời gian: {notification.CreatedAt:dd/MM/yyyy HH:mm}</p>
                    <hr style='margin: 20px 0; border: none; border-top: 1px solid #eee;' />
                    <p style='color: #999; font-size: 12px;'>Đây là email thông báo tự động từ CanPany.</p>
                </div>";
            
            await _emailService.SendEmailAsync(user.Email, subject, body, isHtml: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to send email notification: {ex.Message}");
        }
    }

    public Task<bool> MarkAsReadAsync(string id)
        => _repo.MarkAsReadAsync(id);

    public Task<bool> DeleteAsync(string id)
        => _repo.DeleteAsync(id);
}
