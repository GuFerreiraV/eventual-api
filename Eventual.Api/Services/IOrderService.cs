using Eventual.Api.Models.Dtos;

namespace Eventual.Api.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(int loteId, CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> PayAsync(int id, PayOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> GetAsync(int id, CancellationToken cancellationToken = default);
}
