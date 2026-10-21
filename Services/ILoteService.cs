using eventual.Models.Dtos;

namespace eventual.Services;

public interface ILoteService
{
    Task<IReadOnlyList<LoteResponse>> ListByEventAsync(int eventId, CancellationToken cancellationToken = default);
    Task<LoteResponse> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<LoteResponse> CreateAsync(int eventId, CreateLoteRequest request, CancellationToken cancellationToken = default);
}
