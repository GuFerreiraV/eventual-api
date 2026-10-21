namespace eventual.Exceptions;

public abstract class PaymentException(string message) : DomainException(message);
