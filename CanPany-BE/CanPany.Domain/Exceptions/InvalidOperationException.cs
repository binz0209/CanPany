namespace CanPany.Domain.Exceptions;

public class InvalidDomainOperationException : DomainException
{
    public string Operation { get; }

    public InvalidDomainOperationException(string operation, string message)
        : base(message)
    {
        Operation = operation;
    }

    public InvalidDomainOperationException(string operation, string message, Exception innerException)
        : base(message, innerException)
    {
        Operation = operation;
    }
}

