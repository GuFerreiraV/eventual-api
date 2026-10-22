namespace Eventual.Api.Exceptions;

public class CheckInClosedException(int eventId)
    : ConflictException($"O evento {eventId} já terminou; o check-in está encerrado.");
