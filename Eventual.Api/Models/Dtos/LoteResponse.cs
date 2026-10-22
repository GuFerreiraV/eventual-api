namespace Eventual.Api.Models.Dtos;

public record LoteResponse(int Id, int EventId, string Name, decimal Price, int Quantity, int Available);
