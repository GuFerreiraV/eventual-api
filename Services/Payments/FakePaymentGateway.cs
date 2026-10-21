namespace eventual.Services.Payments;

// Gateway de testes do MVP: o resultado é escolhido pelo token, como no modo sandbox de gateways reais.
public class FakePaymentGateway : IPaymentGateway
{
    public const string ApprovedToken = "tok_approved";
    public const string DeclinedToken = "tok_declined";

    public Task<PaymentResult> ChargeAsync(int orderId, decimal amount, string paymentToken, CancellationToken cancellationToken = default)
    {
        var result = paymentToken switch
        {
            ApprovedToken => new PaymentResult(true),
            DeclinedToken => new PaymentResult(false, "Pagamento recusado pelo emissor do cartão."),
            _ => new PaymentResult(false, "Token de pagamento inválido.")
        };

        return Task.FromResult(result);
    }
}
