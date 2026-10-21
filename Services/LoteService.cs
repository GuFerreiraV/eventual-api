using System.Linq.Expressions;
using eventual.Data;
using eventual.Exceptions;
using eventual.Models;
using eventual.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace eventual.Services;

public class LoteService(EventualDbContext context) : ILoteService
{
    public async Task<IReadOnlyList<LoteResponse>> ListByEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        if (!await context.Events.AnyAsync(e => e.Id == eventId, cancellationToken))
            throw new EventNotFoundException(eventId);

        return await context.Lotes
            .Where(l => l.EventId == eventId)
            .OrderBy(l => l.Id)
            .Select(ToResponse(DateTime.UtcNow))
            .ToListAsync(cancellationToken);
    }

    public async Task<LoteResponse> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Lotes
            .Where(l => l.Id == id)
            .Select(ToResponse(DateTime.UtcNow))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new LoteNotFoundException(id);
    }

    public async Task<LoteResponse> CreateAsync(int eventId, CreateLoteRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.Events.AnyAsync(e => e.Id == eventId, cancellationToken))
            throw new EventNotFoundException(eventId);

        var lote = new Lote
        {
            EventId = eventId,
            Name = request.Name,
            Price = request.Price!.Value,
            Quantity = request.Quantity!.Value
        };

        context.Lotes.Add(lote);
        await context.SaveChangesAsync(cancellationToken);

        // Lote recém-criado não tem pedidos: tudo está disponível.
        return new LoteResponse(lote.Id, lote.EventId, lote.Name, lote.Price, lote.Quantity, lote.Quantity);
    }

    // Disponível = Quantity − pedidos que ainda seguram vaga (OrderRules.HoldsStock).
    private static Expression<Func<Lote, LoteResponse>> ToResponse(DateTime now)
    {
        var holdsStock = OrderRules.HoldsStock(now);
        return l => new LoteResponse(
            l.Id,
            l.EventId,
            l.Name,
            l.Price,
            l.Quantity,
            l.Quantity - l.Orders.AsQueryable().Where(holdsStock).Sum(o => o.Quantity));
    }
}
