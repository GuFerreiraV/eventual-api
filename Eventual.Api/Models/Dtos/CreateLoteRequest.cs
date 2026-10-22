using System.ComponentModel.DataAnnotations;

namespace Eventual.Api.Models.Dtos;

public record CreateLoteRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, Range(0, double.MaxValue)]
    public decimal? Price { get; init; }

    [Required, Range(1, int.MaxValue)]
    public int? Quantity { get; init; }
}
