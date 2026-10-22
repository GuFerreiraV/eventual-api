using System.ComponentModel.DataAnnotations;

namespace Eventual.Api.Models.Dtos;

public record CreateOrderRequest
{
    [Required, Range(1, int.MaxValue)]
    public int? Quantity { get; init; }

    [Required, MaxLength(200)]
    public string BuyerName { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string BuyerEmail { get; init; } = string.Empty;
}
