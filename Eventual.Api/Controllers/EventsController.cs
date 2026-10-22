using Eventual.Api.Models.Dtos;
using Eventual.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eventual.Api.Controllers;

[ApiController]
[Tags("Eventos")]
[Route("events")]
public class EventsController(IEventService eventService, ILoteService loteService) : ControllerBase
{
    /// <summary>Lista os eventos (resumo, sem lotes), ordenados pela data de início.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EventSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => Ok(await eventService.ListAsync(cancellationToken));

    /// <summary>Detalha um evento com seus lotes e as vagas disponíveis de cada um.</summary>
    [HttpGet("{eventId:int}")]
    [ProducesResponseType<EventDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int eventId, CancellationToken cancellationToken)
        => Ok(await eventService.GetAsync(eventId, cancellationToken));

    /// <summary>Cria um evento. O término precisa ser posterior ao início.</summary>
    [HttpPost]
    [ProducesResponseType<EventDetailResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var created = await eventService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { eventId = created.Id }, created);
    }

    /// <summary>Lista os lotes de um evento.</summary>
    [HttpGet("{eventId:int}/lotes")]
    [ProducesResponseType<IReadOnlyList<LoteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListLotes(int eventId, CancellationToken cancellationToken)
        => Ok(await loteService.ListByEventAsync(eventId, cancellationToken));

    /// <summary>Cria um lote de ingressos no evento.</summary>
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
