using System.Net;
using System.Net.Http.Json;
using Eventual.Api.Models;
using Eventual.Api.Models.Dtos;
using Eventual.Tests.Infrastructure;
using Shouldly;

namespace Eventual.Tests;

public class PaymentTests : ApiTestBase
{
    [Fact]
    public async Task Pagamento_aprovado_gera_um_ingresso_por_unidade()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id);
        var order = await CreateOrderAsync(lote.Id, quantity: 3);

        var response = await PayAsync(order.Id);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var paid = (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
        paid.Status.ShouldBe(OrderStatus.Paid);
        paid.Tickets.Count.ShouldBe(3);
        paid.Tickets.Select(t => t.Code).Distinct().Count().ShouldBe(3);
    }

    [Fact]
    public async Task Recusa_mantem_o_pedido_pendente_e_permite_nova_tentativa()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id);
        var order = await CreateOrderAsync(lote.Id, quantity: 2);

        var declined = await PayAsync(order.Id, "tok_declined");
        declined.StatusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        (await GetOrderAsync(order.Id)).Status.ShouldBe(OrderStatus.Pending);

        var approved = await PayAsync(order.Id, "tok_approved");
        approved.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetOrderAsync(order.Id)).Tickets.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Pagar_de_novo_devolve_os_mesmos_ingressos()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id);
        var paid = await CreatePaidOrderAsync(lote.Id, quantity: 2);

        var again = await PayAsync(paid.Id);

        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        var repeated = (await again.Content.ReadFromJsonAsync<OrderResponse>())!;
        repeated.Tickets.Select(t => t.Code).ShouldBe(paid.Tickets.Select(t => t.Code), ignoreOrder: true);
    }

    [Fact]
    public async Task Pagamento_sem_token_devolve_400()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id);
        var order = await CreateOrderAsync(lote.Id);

        var response = await Client.PostAsJsonAsync($"/orders/{order.Id}/payment", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Pagamentos_simultaneos_geram_ingressos_uma_unica_vez()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id);
        var order = await CreateOrderAsync(lote.Id, quantity: 3);

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => PayAsync(order.Id)));

        responses.ShouldAllBe(r => r.StatusCode == HttpStatusCode.OK);
        (await GetOrderAsync(order.Id)).Tickets.Count.ShouldBe(3);
    }
}
