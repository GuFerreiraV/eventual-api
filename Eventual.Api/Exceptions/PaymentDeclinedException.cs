namespace Eventual.Api.Exceptions;

public class PaymentDeclinedException(int orderId, string reason)
    : PaymentException($"Pagamento do pedido {orderId} recusado: {reason} O pedido continua reservado até expirar; tente novamente.");
