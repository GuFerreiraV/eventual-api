namespace Eventual.Api.Exceptions;

public class OrderLimitExceededException(int requested, int max)
    : DomainException($"Um pedido pode ter no máximo {max} ingresso(s); foram solicitados {requested}.");
