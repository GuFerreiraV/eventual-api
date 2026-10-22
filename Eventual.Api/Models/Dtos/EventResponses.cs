namespace Eventual.Api.Models.Dtos;

public record EventSummaryResponse(int Id, string Name, string Local, DateTime Start, DateTime End);

public record EventDetailResponse(
    int Id,
    string Name,
    string Description,
    string Local,
    DateTime Start,
    DateTime End,
    IReadOnlyList<LoteResponse> Lotes);
