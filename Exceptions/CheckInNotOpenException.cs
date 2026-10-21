namespace eventual.Exceptions;

public class CheckInNotOpenException(int eventId, DateTime opensAt)
    : ConflictException($"O check-in do evento {eventId} abre às {opensAt:HH:mm} (UTC) de {opensAt:dd/MM/yyyy}.");
