namespace eventual.Exceptions;

public abstract class NotFoundException(string message) : DomainException(message);
