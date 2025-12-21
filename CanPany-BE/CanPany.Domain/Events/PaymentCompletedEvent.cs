using CanPany.Domain.Entities;

namespace CanPany.Domain.Events;

public class PaymentCompletedEvent : BaseDomainEvent
{
    public Payment Payment { get; }
    public string UserId { get; }
    public decimal Amount { get; }

    public PaymentCompletedEvent(Payment payment, string userId, decimal amount)
    {
        Payment = payment;
        UserId = userId;
        Amount = amount;
    }
}

