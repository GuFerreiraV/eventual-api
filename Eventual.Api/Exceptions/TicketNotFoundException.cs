namespace Eventual.Api.Exceptions;

public class TicketNotFoundException(Guid code)
    : NotFoundException($"Ingresso {code} não encontrado.");
