using eventual.Models.Dtos;
using eventual.Services;
using Microsoft.AspNetCore.Mvc;

namespace eventual.Controllers;

[ApiController]
[Route("events")]
public class EventsController(IEventService eventService, ILoteService loteService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EventSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => Ok(await eventService.ListAsync(cancellationToken));

    [HttpGet("{eventId:int}")]
    [ProducesResponseType<EventDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int eventId, CancellationToken cancellationToken)
        => Ok(await eventService.GetAsync(eventId, cancellationToken));

    [HttpPost]
    [ProducesResponseType<EventDetailResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var created = await eventService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { eventId = created.Id }, created);
    }

    [HttpGet("{eventId:int}/lotes")]
    [ProducesResponseType<IReadOnlyList<LoteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListLotes(int eventId, CancellationToken cancellationToken)
        => Ok(await loteService.ListByEventAsync(eventId, cancellationToken));

    [HttpPost("{eventId:int}/lotes")]
    [ProducesResponseType<LoteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateLote(int eventId, CreateLoteRequest request, CancellationToken cancellationToken)
    {
        var created = await loteService.CreateAsync(eventId, request, cancellationToken);
        return CreatedAtAction(nameof(LotesController.Get), "Lotes", new { id = created.Id }, created);
    }
}
