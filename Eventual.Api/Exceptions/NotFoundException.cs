namespace Eventual.Api.Exceptions;

public abstract class NotFoundException(string message) : DomainException(message);
