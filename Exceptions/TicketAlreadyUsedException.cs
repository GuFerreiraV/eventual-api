namespace eventual.Exceptions;

public class TicketAlreadyUsedException(Guid code) 
    : DomainException($"Erro ao realizar o check-in do ingresso {code}, o mesmo já se encontra utilizado.");