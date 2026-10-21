using eventual.Models.Dtos;
using eventual.Services;
using Microsoft.AspNetCore.Mvc;

namespace eventual.Controllers;

[ApiController]
public class CheckInController(ICheckInService checkInService) : ControllerBase
{
    [HttpPost("events/{eventId:int}/check-in")]
    [ProducesResponseType<CheckInResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckIn(int eventId, CheckInRequest request, CancellationToken cancellationToken)
        => Ok(await checkInService.CheckInAsync(eventId, request, cancellationToken));
}
