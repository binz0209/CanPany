using CanPany.Domain.Entities;

namespace CanPany.Domain.Events;

public class NotificationCreatedEvent : BaseDomainEvent
{
    public Notification Notification { get; }

    public NotificationCreatedEvent(Notification notification)
    {
        Notification = notification;
    }
}

