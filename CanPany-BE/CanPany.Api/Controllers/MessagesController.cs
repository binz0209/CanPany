// CanPany.Api/Controllers/MessagesController.cs
using CanPany.Application.DTOs.Messages;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _svc;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly IUserSettingsService _userSettingsService;
    private readonly IHubContext<MessageHub> _messageHub;

    public MessagesController(
        IMessageService svc,
        INotificationService notificationService,
        IUserService userService,
        IUserSettingsService userSettingsService,
        IHubContext<MessageHub> messageHub)
    {
        _svc = svc;
        _notificationService = notificationService;
        _userService = userService;
        _userSettingsService = userSettingsService;
        _messageHub = messageHub;
        Console.WriteLine("✅ [MessagesController] Controller initialized");
    }

    private string? GetUserId()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? User.FindFirst("sub")?.Value
           ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

    // NEW: trả toàn bộ tin nhắn của user
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyMessages()
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var list = await _svc.GetByUserAsync(userId);
        return Ok(list);
    }

    // NEW: trả danh sách hội thoại đã group (không tạo DTO ngoài)
    [Authorize]
    [HttpGet("my-conversations")]
    public async Task<IActionResult> MyConversations()
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var convos = await _svc.GetConversationsForUserAsync(userId);

        // Trả object ẩn danh đúng shape cho FE
        var result = convos.Select(c => new {
            conversationKey = c.ConversationKey,
            partnerId = c.PartnerId,
            lastMessage = c.LastMessage,
            lastAt = c.LastAt,
            unreadCount = c.UnreadCount
        });

        return Ok(result);
    }

    [Authorize]
    [HttpGet("thread/{conversationKey}")]
    public async Task<IActionResult> Thread(string conversationKey)
        => Ok(await _svc.GetByConversationAsync(conversationKey));

    [Authorize]
    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> ByProject(string projectId)
        => Ok(await _svc.GetByProjectAsync(projectId));

    public class SendMessageRequest
    {
        public string? ConversationKey { get; set; }    // optional
        public string ReceiverId { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string? ProjectId { get; set; }          // optional
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest body)
    {
        Console.WriteLine($"📨 [MessagesController.Send] Received request. ReceiverId: {body?.ReceiverId}, Text length: {body?.Text?.Length ?? 0}");
        
        var senderId = GetUserId();
        if (string.IsNullOrEmpty(senderId))
        {
            Console.WriteLine("❌ [MessagesController.Send] Unauthorized - no senderId");
            return Unauthorized();
        }
        
        Console.WriteLine($"📨 [MessagesController.Send] SenderId: {senderId}");

        if (body == null || string.IsNullOrWhiteSpace(body.ReceiverId) || string.IsNullOrWhiteSpace(body.Text))
        {
            Console.WriteLine("❌ [MessagesController.Send] BadRequest - missing receiverId or text");
            return BadRequest("receiverId và text là bắt buộc.");
        }

        // Nếu FE truyền sẵn conversationKey thì dùng luôn; nếu không -> build chuẩn 3 phần
        var convKey = !string.IsNullOrWhiteSpace(body.ConversationKey)
            ? body.ConversationKey!
            : BuildKey(body.ProjectId, senderId, body.ReceiverId);

        Console.WriteLine($"📨 [MessagesController.Send] ConversationKey: {convKey}");

        var msg = new Message
        {
            SenderId = senderId,
            ReceiverId = body.ReceiverId,
            ProjectId = string.IsNullOrWhiteSpace(body.ProjectId) ? null : body.ProjectId,
            Text = body.Text,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            ConversationKey = convKey
        };

        Console.WriteLine($"📨 [MessagesController.Send] Saving message...");
        var saved = await _svc.SendAsync(msg);
        Console.WriteLine($"✅ [MessagesController.Send] Message saved. Id: {saved.Id}");

        // 📡 Gửi tin nhắn real-time qua SignalR cho cả người gửi và người nhận
        try
        {
            var normalizedMessage = new
            {
                id = saved.Id,
                conversationKey = convKey,
                senderId = saved.SenderId,
                receiverId = saved.ReceiverId,
                projectId = saved.ProjectId,
                text = saved.Text,
                createdAt = saved.CreatedAt,
                isRead = saved.IsRead
            };

            // Gửi cho người nhận
            await _messageHub.Clients.User(body.ReceiverId)
                .SendAsync("ReceiveMessage", normalizedMessage);
            Console.WriteLine($"📡 [SignalR] Message sent to receiver: {body.ReceiverId}");

            // Gửi cho người gửi (để sync real-time)
            await _messageHub.Clients.User(senderId)
                .SendAsync("ReceiveMessage", normalizedMessage);
            Console.WriteLine($"📡 [SignalR] Message sent to sender: {senderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ [SignalR] Failed to send real-time message: {ex.Message}");
        }

        // 🔔 Gửi notification "tin nhắn mới" cho người nhận
        Console.WriteLine($"📩 [MessagesController.Send] Starting notification creation...");
        try
        {
            // Kiểm tra settings của người nhận
            var receiverSettings = await _userSettingsService.GetByUserIdAsync(body.ReceiverId);
            if (receiverSettings?.NotificationSettings?.MessageNotifications == false)
            {
                Console.WriteLine($"⚠️ [MessagesController.Send] Receiver has message notifications disabled. Skipping notification.");
                return Ok(saved);
            }

            Console.WriteLine($"📩 [MessagesController.Send] Creating notification for message. ReceiverId: {body.ReceiverId}, SenderId: {senderId}");
            
            var sender = await _userService.GetByIdAsync(senderId);
            var senderName = sender?.FullName ?? "Người dùng";
            Console.WriteLine($"📩 Sender found: {senderName}");

            // Làm sạch text để hiển thị trong notification (loại bỏ HTML nếu có)
            var notificationText = body.Text.Length > 100 
                ? body.Text.Substring(0, 100) + "..." 
                : body.Text;
            
            // Loại bỏ HTML tags cơ bản
            notificationText = Regex.Replace(
                notificationText, 
                "<.*?>", 
                string.Empty
            );

            var payload = JsonSerializer.Serialize(new
            {
                conversationKey = convKey,
                messageId = saved.Id,
                senderId = senderId,
                receiverId = body.ReceiverId,
                projectId = body.ProjectId,
                action = "NewMessage"
            });

            var notification = new Notification
            {
                UserId = body.ReceiverId,
                Type = "NewMessage",
                Title = "Bạn có tin nhắn mới",
                Message = $"{senderName}: {notificationText}",
                Payload = payload,
                CreatedAt = DateTime.UtcNow
            };

            Console.WriteLine($"📩 Notification created. Type: {notification.Type}, UserId: {notification.UserId}");
            
            var createdNotification = await _notificationService.CreateAsync(notification);
            Console.WriteLine($"✅ Message notification created successfully. Id: {createdNotification.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [MessagesController.Send] Failed to send message notification: {ex.Message}");
            Console.WriteLine($"❌ [MessagesController.Send] Stack trace: {ex.StackTrace}");
            Console.WriteLine($"❌ [MessagesController.Send] Inner exception: {ex.InnerException?.Message}");
        }

        Console.WriteLine($"📨 [MessagesController.Send] Returning response");
        return Ok(saved);

        // local helper: đảm bảo cùng format với Service/Repo
        static string BuildKey(string? projectId, string a, string b)
        {
            var u1 = string.CompareOrdinal(a, b) <= 0 ? a : b;
            var u2 = ReferenceEquals(u1, a) ? b : a;
            var pid = string.IsNullOrWhiteSpace(projectId) ? "null" : projectId;
            return $"{pid}:{u1}:{u2}";
        }
    }


    [Authorize]
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(string id)
        => Ok(await _svc.MarkAsReadAsync(id));

    [Authorize]
    [HttpPost("conversation/{conversationKey}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(string conversationKey)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var count = await _svc.MarkAllAsReadInConversationAsync(conversationKey, userId);
        return Ok(new { count });
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("proposal")]
    public async Task<IActionResult> CreateProposalMessage([FromBody] ProposalMessageCreateDto dto)
    {
        Console.WriteLine("== CreateProposalMessage payload ==");
        Console.WriteLine(JsonSerializer.Serialize(dto));

        if (string.IsNullOrWhiteSpace(dto.ProjectId) ||
            string.IsNullOrWhiteSpace(dto.ProposalId) ||
            string.IsNullOrWhiteSpace(dto.ClientId) ||
            string.IsNullOrWhiteSpace(dto.FreelancerId))
        {
            return BadRequest(new { message = "Missing required fields (ProjectId/ProposalId/ClientId/FreelancerId)." });
        }

        var msg = await _svc.CreateProposalMessageAsync(
            dto.ProjectId,
            dto.ProposalId,
            dto.ClientId,
            dto.FreelancerId,
            dto.ProjectTitle ?? string.Empty,
            dto.ClientName ?? string.Empty,
            dto.FreelancerName ?? string.Empty,
            dto.CoverLetter ?? string.Empty,
            dto.BidAmount
        );

        return Ok(msg);
    }
}
