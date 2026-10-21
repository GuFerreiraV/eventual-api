using eventual.Data;
using eventual.Exceptions;
using eventual.Models;
using eventual.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace eventual.Services;

public class EventService(EventualDbContext context, ILoteService loteService) : IEventService
{
    public async Task<IReadOnlyList<EventSummaryResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await context.Events
            .OrderBy(e => e.Start)
            .Select(e => new EventSummaryResponse(e.Id, e.Name, e.Local, e.Start, e.End))
            .ToListAsync(cancellationToken);
    }

    public async Task<EventDetailResponse> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var ev = await context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new EventNotFoundException(id);

        var lotes = await loteService.ListByEventAsync(id, cancellationToken);
        return ToDetail(ev, lotes);
    }

    public async Task<EventDetailResponse> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default)
    {
        var ev = new Event
        {
            Name = request.Name,
            Description = request.Description,
            Local = request.Local,
            Start = request.Start!.Value,
            End = request.End!.Value
        };

        context.Events.Add(ev);
        await context.SaveChangesAsync(cancellationToken);

        return ToDetail(ev, []);
    }

    private static EventDetailResponse ToDetail(Event ev, IReadOnlyList<LoteResponse> lotes) =>
        new(ev.Id, ev.Name, ev.Description, ev.Local, ev.Start, ev.End, lotes);
}
