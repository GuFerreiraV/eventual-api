namespace eventual.Exceptions;

public class TicketAlreadyUsedException(Guid code, DateTime checkedInAt)
    : ConflictException($"Erro ao realizar o check-in do ingresso {code}, o mesmo já foi utilizado às {checkedInAt:HH:mm} (UTC) de {checkedInAt:dd/MM/yyyy}.");
