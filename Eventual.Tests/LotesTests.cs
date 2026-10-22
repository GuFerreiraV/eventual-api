using System.Net;
using System.Net.Http.Json;
using Eventual.Tests.Infrastructure;
using Shouldly;

namespace Eventual.Tests;

public class LotesTests : ApiTestBase
{
    [Fact]
    public async Task Lote_novo_tem_todas_as_vagas_disponiveis()
    {
        var ev = await CreateEventAsync();

        var lote = await CreateLoteAsync(ev.Id, quantity: 10);

        lote.Available.ShouldBe(10);
    }

    [Fact]
    public async Task Criar_lote_em_evento_inexistente_devolve_404()
    {
        var response = await Client.PostAsJsonAsync("/events/99/lotes", new { name = "Pista", price = 10, quantity = 5 });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Lote_inexistente_devolve_404()
    {
        var response = await Client.GetAsync("/lotes/99");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadProblemAsync(response)).Detail.ShouldBe("Lote 99 não encontrado.");
    }

    [Fact]
    public async Task Reserva_diminui_as_vagas_disponiveis()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id, quantity: 10);

        await CreateOrderAsync(lote.Id, quantity: 2);

        (await GetLoteAsync(lote.Id)).Available.ShouldBe(8);
    }
}
