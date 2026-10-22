using Eventual.Api.Data;
using Eventual.Api.Exceptions;
using Eventual.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Eventual.Api.Services;

public class CheckInService(EventualDbContext context, TimeProvider timeProvider) : ICheckInService
{
    // A portaria abre este tempo antes do início do evento.
    public static readonly TimeSpan OpensBeforeStart = TimeSpan.FromMinutes(30);

    public async Task<CheckInResponse> CheckInAsync(int eventId, CheckInRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code!.Value;

        var ev = await context.Events.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken)
            ?? throw new EventNotFoundException(eventId);

        var ticket = await context.Tickets
            .Include(t => t.Order).ThenInclude(o => o.Lote)
            .FirstOrDefaultAsync(t => t.Code == code, cancellationToken)
            ?? throw new TicketNotFoundException(code);

        // Antes de qualquer outra informação: o ingresso precisa ser desta porta.
        if (ticket.Order.Lote.EventId != eventId)
            throw new TicketNotForEventException(eventId);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var opensAt = ev.Start - OpensBeforeStart;
        if (now < opensAt)
            throw new CheckInNotOpenException(eventId, opensAt);
        if (now >= ev.End)
            throw new CheckInClosedException(eventId);

        if (ticket.CheckedInAt is { } usedAt)
            throw new TicketAlreadyUsedException(code, usedAt);

        ticket.CheckedInAt = now;
        // Trocar a versão faz uma leitura simultânea do mesmo QR falhar ao salvar.
        ticket.Version = Guid.NewGuid();

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Outro leitor registrou a entrada primeiro: esta leitura é barrada.
            context.ChangeTracker.Clear();
            var current = await context.Tickets.AsNoTracking().FirstAsync(t => t.Code == code, cancellationToken);
            throw new TicketAlreadyUsedException(code, current.CheckedInAt ?? now);
        }

        return new CheckInResponse(ticket.Code, ticket.Order.BuyerName, ticket.Order.Lote.Name, now);
    }
}
