namespace Eventual.Api.Exceptions;

public abstract class ConflictException(string message) : DomainException(message);
