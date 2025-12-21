using CanPany.Domain.Entities;

namespace CanPany.Domain.Specifications;

public static class PaymentSpecifications
{
    public static ISpecification<Payment> IsPending()
    {
        return new PendingPaymentSpecification();
    }

    public static ISpecification<Payment> IsCompleted()
    {
        return new CompletedPaymentSpecification();
    }

    public static ISpecification<Payment> IsFailed()
    {
        return new FailedPaymentSpecification();
    }

    public static ISpecification<Payment> HasMinimumAmount(decimal minAmount)
    {
        return new HasMinimumAmountSpecification(minAmount);
    }
}

internal class PendingPaymentSpecification : ISpecification<Payment>
{
    public bool IsSatisfiedBy(Payment entity)
    {
        return entity.Status == "Pending";
    }
}

internal class CompletedPaymentSpecification : ISpecification<Payment>
{
    public bool IsSatisfiedBy(Payment entity)
    {
        return entity.Status == "Paid";
    }
}

internal class FailedPaymentSpecification : ISpecification<Payment>
{
    public bool IsSatisfiedBy(Payment entity)
    {
        return entity.Status == "Failed";
    }
}

internal class HasMinimumAmountSpecification : ISpecification<Payment>
{
    private readonly decimal _minAmount;

    public HasMinimumAmountSpecification(decimal minAmount)
    {
        _minAmount = minAmount;
    }

    public bool IsSatisfiedBy(Payment entity)
    {
        return entity.Amount >= _minAmount;
    }
}

