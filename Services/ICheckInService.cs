using eventual.Models.Dtos;

namespace eventual.Services;

public interface ICheckInService
{
    Task<CheckInResponse> CheckInAsync(int eventId, CheckInRequest request, CancellationToken cancellationToken = default);
}
