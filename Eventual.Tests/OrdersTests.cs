using System.Net;
using Eventual.Api.Models;
using Eventual.Tests.Infrastructure;
using Shouldly;

namespace Eventual.Tests;

public class OrdersTests : ApiTestBase
{
    [Fact]
    public async Task Reserva_nasce_pendente_e_expira_em_15_minutos()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id, price: 80m);

        var order = await CreateOrderAsync(lote.Id, quantity: 2);

        order.Status.ShouldBe(OrderStatus.Pending);
        order.TotalAmount.ShouldBe(160m);
        order.ExpiresAt.ShouldBe(Now.AddMinutes(15));
        order.Tickets.ShouldBeEmpty();
    }

    [Fact]
    public async Task Mais_de_5_ingressos_por_pedido_devolve_400()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id);

        var response = await PostOrderAsync(lote.Id, quantity: 6);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Pedir_mais_do_que_o_disponivel_devolve_409()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id, quantity: 3);
        await CreateOrderAsync(lote.Id, quantity: 2);

        var response = await PostOrderAsync(lote.Id, quantity: 2);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ReadProblemAsync(response)).Detail.ShouldBe($"O lote {lote.Id} tem apenas 1 ingresso(s) disponível(is).");
    }

    [Fact]
    public async Task Reservar_depois_que_o_evento_terminou_devolve_409()
    {
        var ev = await CreateEventAsync(start: Now.AddHours(-2), end: Now.AddHours(1));
        var lote = await CreateLoteAsync(ev.Id);

        Time.Advance(TimeSpan.FromHours(2));
        var response = await PostOrderAsync(lote.Id, quantity: 1);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Pedido_nao_pago_expira_e_devolve_a_vaga()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id, quantity: 10);
        var order = await CreateOrderAsync(lote.Id, quantity: 3);
        (await GetLoteAsync(lote.Id)).Available.ShouldBe(7);

        Time.Advance(TimeSpan.FromMinutes(16));

        (await GetOrderAsync(order.Id)).Status.ShouldBe(OrderStatus.Expired);
        (await GetLoteAsync(lote.Id)).Available.ShouldBe(10);

        var payment = await PayAsync(order.Id);
        payment.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Reservas_simultaneas_nunca_vendem_alem_do_lote()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id, quantity: 3);

        var responses = await Task.WhenAll(
            Enumerable.Range(0, 20).Select(_ => PostOrderAsync(lote.Id, quantity: 1)));

        responses.Count(r => r.StatusCode == HttpStatusCode.Created).ShouldBe(3);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(17);
        (await GetLoteAsync(lote.Id)).Available.ShouldBe(0);
    }
}
