namespace eventual.Exceptions;

public class EventNotFoundException(int eventId) 
    : NotFoundException($"Evento {eventId} não encontrado.");