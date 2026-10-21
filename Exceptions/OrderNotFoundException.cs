namespace eventual.Exceptions;

public class OrderNotFoundException(int orderId)
    : NotFoundException($"Pedido {orderId} não encontrado.");
