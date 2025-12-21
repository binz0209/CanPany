using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using CanPany.Infrastructure.Messaging.SignalR.Hubs;

namespace CanPany.Infrastructure.Messaging.SignalR;

public class SignalRRealtimeService : IRealtimeService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRRealtimeService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(string userId, Notification notification)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("⚠️ SignalR SendToUserAsync: userId is null or empty — skip sending");
            return;
        }

        if (notification == null)
        {
            Console.WriteLine($"⚠️ SignalR SendToUserAsync: notification is null for user {userId}");
            return;
        }

        try
        {
            Console.WriteLine($"📡 [SignalRRealtimeService] Sending notification to userId={userId}, Type={notification.Type}, Id={notification.Id}");
            
            await _hubContext.Clients.User(userId)
                .SendAsync("ReceiveNotification", notification);
            
            Console.WriteLine($"✅ [SignalRRealtimeService] Notification sent successfully to userId={userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SignalRRealtimeService] Failed to send notification to {userId}: {ex.Message}");
            Console.WriteLine($"❌ [SignalRRealtimeService] Stack trace: {ex.StackTrace}");
        }
    }
}

