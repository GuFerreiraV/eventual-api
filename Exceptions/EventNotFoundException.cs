namespace eventual.Exceptions;

public class EventNotFoundException(int eventId) 
    : DomainException($"Evento {eventId} não encontrado.");