using Eventual.Api.Models.Dtos;

namespace Eventual.Api.Services;

public interface ICheckInService
{
    Task<CheckInResponse> CheckInAsync(int eventId, CheckInRequest request, CancellationToken cancellationToken = default);
}
