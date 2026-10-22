using System.ComponentModel.DataAnnotations;

namespace Eventual.Api.Models.Dtos;

public record PayOrderRequest
{
    /// <summary>Token de teste: "tok_approved" aprova, "tok_declined" recusa.</summary>
    [Required]
    public string PaymentToken { get; init; } = string.Empty;
}
