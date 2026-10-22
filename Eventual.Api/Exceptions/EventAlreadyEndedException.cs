namespace Eventual.Api.Exceptions;

public class EventAlreadyEndedException(int eventId)
    : ConflictException($"O evento {eventId} já terminou; não é mais possível comprar ingressos.");
