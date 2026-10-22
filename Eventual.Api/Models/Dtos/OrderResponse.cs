namespace Eventual.Api.Models.Dtos;

public record OrderResponse(
    int Id,
    int LoteId,
    int Quantity,
    decimal TotalAmount,
    string BuyerName,
    string BuyerEmail,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    IReadOnlyList<TicketResponse> Tickets);

public record TicketResponse(int Id, Guid Code, DateTime? CheckedInAt);
