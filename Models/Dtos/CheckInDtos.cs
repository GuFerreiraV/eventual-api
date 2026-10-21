using System.ComponentModel.DataAnnotations;

namespace eventual.Models.Dtos;

public record CheckInRequest
{
    /// <summary>Código do ingresso (conteúdo do QR code).</summary>
    [Required]
    public Guid? Code { get; init; }
}

// Só o que a portaria precisa para conferir a entrada: sem e-mail, valor ou id do pedido.
public record CheckInResponse(Guid Code, string BuyerName, string LoteName, DateTime CheckedInAt);
