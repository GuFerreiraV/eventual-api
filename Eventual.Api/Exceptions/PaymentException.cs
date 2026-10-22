namespace Eventual.Api.Exceptions;

public abstract class PaymentException(string message) : DomainException(message);
