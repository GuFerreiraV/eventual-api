using Eventual.Api.Models.Dtos;

namespace Eventual.Api.Services;

public interface IEventService
{
    Task<IReadOnlyList<EventSummaryResponse>> ListAsync(CancellationToken cancellationToken = default);
    Task<EventDetailResponse> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<EventDetailResponse> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default);
}
