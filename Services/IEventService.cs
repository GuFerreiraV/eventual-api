using eventual.Models.Dtos;

namespace eventual.Services;

public interface IEventService
{
    Task<IReadOnlyList<EventSummaryResponse>> ListAsync(CancellationToken cancellationToken = default);
    Task<EventDetailResponse> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<EventDetailResponse> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default);
}
