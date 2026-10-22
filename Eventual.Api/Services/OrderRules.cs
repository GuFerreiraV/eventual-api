using System.Linq.Expressions;
using Eventual.Api.Models;

namespace Eventual.Api.Services;

public static class OrderRules
{
    public const int MaxTicketsPerOrder = 5;
    public static readonly TimeSpan ReservationDuration = TimeSpan.FromMinutes(15);

    // Um pedido segura vaga se está pago ou se ainda está pendente dentro do prazo (expiração preguiçosa).
    public static Expression<Func<Order, bool>> HoldsStock(DateTime now) =>
        o => o.Status == OrderStatus.Paid
          || (o.Status == OrderStatus.Pending && o.ExpiresAt > now);

    // Status visto pelo cliente: um pendente vencido aparece como Expired, sem gravar nada.
    public static OrderStatus EffectiveStatus(Order order, DateTime now) =>
        order.Status == OrderStatus.Pending && order.ExpiresAt <= now
            ? OrderStatus.Expired
            : order.Status;
}
