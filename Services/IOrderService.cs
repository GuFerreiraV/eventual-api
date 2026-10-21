using eventual.Models.Dtos;

namespace eventual.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(int loteId, CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> PayAsync(int id, PayOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> GetAsync(int id, CancellationToken cancellationToken = default);
}
