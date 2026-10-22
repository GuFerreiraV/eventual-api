using Eventual.Api.Models.Dtos;
using Eventual.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eventual.Api.Controllers;

[ApiController]
[Tags("Check-in")]
public class CheckInController(ICheckInService checkInService) : ControllerBase
{
    /// <summary>Registra a entrada de um ingresso na portaria do evento (abre 30 min antes do início).</summary>
    [HttpPost("events/{eventId:int}/check-in")]
    [ProducesResponseType<CheckInResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckIn(int eventId, CheckInRequest request, CancellationToken cancellationToken)
        => Ok(await checkInService.CheckInAsync(eventId, request, cancellationToken));
}
