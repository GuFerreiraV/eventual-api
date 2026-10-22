namespace Eventual.Api.Exceptions;

public class OrderExpiredException(int orderId)
    : ConflictException($"O pedido {orderId} expirou; a reserva foi liberada. Faça um novo pedido.");
