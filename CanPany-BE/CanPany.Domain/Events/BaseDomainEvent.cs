namespace CanPany.Domain.Events;

public abstract class BaseDomainEvent
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    public string EventId { get; protected set; } = Guid.NewGuid().ToString();
}

