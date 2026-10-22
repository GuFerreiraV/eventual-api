namespace Eventual.Api.Exceptions;

public class ReservationConflictException(int loteId)
    : ConflictException($"Muitas reservas simultâneas no lote {loteId}. Tente novamente.");
