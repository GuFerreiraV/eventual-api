using System.ComponentModel.DataAnnotations;

namespace Eventual.Api.Models.Dtos;

public record CreateEventRequest : IValidatableObject
{
    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required, MaxLength(200)]
    public string Local { get; init; } = string.Empty;

    [Required]
    public DateTime? Start { get; init; }

    [Required]
    public DateTime? End { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Start is not null && End is not null && End <= Start)
        {
            yield return new ValidationResult(
                "A data de término deve ser posterior à data de início.",
                [nameof(End)]);
        }
    }
}
