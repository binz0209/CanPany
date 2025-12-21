using CanPany.Domain.Entities;

namespace CanPany.Domain.Events;

public class UserCreatedEvent : BaseDomainEvent
{
    public User User { get; }

    public UserCreatedEvent(User user)
    {
        User = user;
    }
}

