namespace Eventual.Api.Exceptions;

public class TicketNotForEventException(int eventId)
    : DomainException($"Este ingresso não pertence ao evento {eventId}.");
