using Eventual.Api.Data;
using Eventual.Api.Exceptions;
using Eventual.Api.Models;
using Eventual.Api.Models.Dtos;
using Eventual.Api.Services.Payments;
using Microsoft.EntityFrameworkCore;

namespace Eventual.Api.Services;

public class OrderService(EventualDbContext context, IPaymentGateway paymentGateway, TimeProvider timeProvider) : IOrderService
{
    private const int MaxAttempts = 3;

    public async Task<OrderResponse> CreateAsync(int loteId, CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var quantity = request.Quantity!.Value;
        if (quantity > OrderRules.MaxTicketsPerOrder)
            throw new OrderLimitExceededException(quantity, OrderRules.MaxTicketsPerOrder);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;

            var lote = await context.Lotes
                .Include(l => l.Event)
                .FirstOrDefaultAsync(l => l.Id == loteId, cancellationToken)
                ?? throw new LoteNotFoundException(loteId);

            if (lote.Event.End <= now)
                throw new EventAlreadyEndedException(lote.EventId);

            var held = await context.Orders
                .Where(o => o.LoteId == loteId)
                .Where(OrderRules.HoldsStock(now))
                .SumAsync(o => o.Quantity, cancellationToken);

            var available = lote.Quantity - held;
            if (available < quantity)
                throw new LoteSoldOutException(loteId, available);

            var order = new Order
            {
                LoteId = loteId,
                Quantity = quantity,
                TotalAmount = lote.Price * quantity,
                BuyerName = request.BuyerName,
                BuyerEmail = request.BuyerEmail,
                Status = OrderStatus.Pending,
                CreatedAt = now,
                ExpiresAt = now + OrderRules.ReservationDuration
            };
            context.Orders.Add(order);

            // Trocar a versão faz o SaveChanges falhar se outra reserva salvou este lote depois da nossa leitura.
            lote.Version = Guid.NewGuid();

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return ToResponse(order, now);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Descarta o que foi lido e tenta de novo com dados frescos.
                context.ChangeTracker.Clear();
            }
        }

        throw new ReservationConflictException(loteId);
    }

    public async Task<OrderResponse> PayAsync(int id, PayOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders
            .Include(o => o.Tickets)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new OrderNotFoundException(id);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Pagar de novo um pedido já pago não é erro: devolve o mesmo resultado (idempotente).
        if (order.Status == OrderStatus.Paid)
            return ToResponse(order, now);

        if (OrderRules.EffectiveStatus(order, now) == OrderStatus.Expired)
        {
            await MarkExpiredAsync(order, cancellationToken);
            throw new OrderExpiredException(id);
        }

        // O id do pedido vai como chave de idempotência para o gateway.
        var result = await paymentGateway.ChargeAsync(order.Id, order.TotalAmount, request.PaymentToken, cancellationToken);
        if (!result.Approved)
            throw new PaymentDeclinedException(order.Id, result.Reason ?? "motivo não informado.");

        order.Status = OrderStatus.Paid;
        for (var i = 0; i < order.Quantity; i++)
            order.Tickets.Add(new Ticket { Code = Guid.NewGuid() });

        // Trocar a versão faz um pagamento simultâneo do mesmo pedido falhar ao salvar.
        order.Version = Guid.NewGuid();

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return ToResponse(order, now);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Outra requisição salvou primeiro. Se foi o pagamento, devolve o pedido pago, sem erro para o usuário.
            context.ChangeTracker.Clear();
            var current = await context.Orders
                .AsNoTracking()
                .Include(o => o.Tickets)
                .FirstAsync(o => o.Id == id, cancellationToken);

            if (current.Status == OrderStatus.Paid)
                return ToResponse(current, timeProvider.GetUtcNow().UtcDateTime);

            throw;
        }
    }

    private async Task MarkExpiredAsync(Order order, CancellationToken cancellationToken)
    {
        if (order.Status == OrderStatus.Expired)
            return;

        order.Status = OrderStatus.Expired;
        order.Version = Guid.NewGuid();
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Outra requisição já atualizou o pedido; ele continua vencido de qualquer forma.
        }
    }

    public async Task<OrderResponse> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Include(o => o.Tickets)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new OrderNotFoundException(id);

        return ToResponse(order, timeProvider.GetUtcNow().UtcDateTime);
    }

    private static OrderResponse ToResponse(Order order, DateTime now) =>
        new(order.Id,
            order.LoteId,
            order.Quantity,
            order.TotalAmount,
            order.BuyerName,
            order.BuyerEmail,
            OrderRules.EffectiveStatus(order, now),
            order.CreatedAt,
            order.ExpiresAt,
            order.Tickets.Select(t => new TicketResponse(t.Id, t.Code, t.CheckedInAt)).ToList());
}
