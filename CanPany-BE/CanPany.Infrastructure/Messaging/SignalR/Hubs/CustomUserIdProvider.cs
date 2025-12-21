using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CanPany.Infrastructure.Messaging.SignalR.Hubs;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var id =
            connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? connection.User?.FindFirst("sub")?.Value
            ?? connection.User?.FindFirst("userId")?.Value
            ?? connection.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(id))
        {
            Console.WriteLine("⚠️ [SignalR] Missing userId in claims");
            return null;
        }

        Console.WriteLine($"🔗 [SignalR] Mapping connection for userId={id}, ConnectionId={connection.ConnectionId}");

        return id;
    }
}

