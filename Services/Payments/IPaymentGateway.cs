namespace eventual.Services.Payments;

public record PaymentResult(bool Approved, string? Reason = null);

public interface IPaymentGateway
{
    // orderId funciona como chave de idempotência: o gateway real não cobra duas vezes o mesmo pedido.
    Task<PaymentResult> ChargeAsync(int orderId, decimal amount, string paymentToken, CancellationToken cancellationToken = default);
}
