using System.Net;
using System.Net.Http.Json;
using Eventual.Api.Models.Dtos;
using Eventual.Tests.Infrastructure;
using Shouldly;

namespace Eventual.Tests;

public class CheckInTests : ApiTestBase
{
    [Fact]
    public async Task Check_in_valido_mostra_comprador_e_lote()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id, name: "Camarote");
        var paid = await CreatePaidOrderAsync(lote.Id);
        var code = paid.Tickets[0].Code;

        var response = await CheckInAsync(ev.Id, code);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<CheckInResponse>())!;
        body.BuyerName.ShouldBe("Ana Souza");
        body.LoteName.ShouldBe("Camarote");
        body.CheckedInAt.ShouldBe(Now);
    }

    [Fact]
    public async Task Segunda_leitura_do_mesmo_ingresso_devolve_409_com_o_horario()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id);
        var code = (await CreatePaidOrderAsync(lote.Id)).Tickets[0].Code;
        await CheckInAsync(ev.Id, code);

        var second = await CheckInAsync(ev.Id, code);

        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ReadProblemAsync(second)).Detail!.ShouldContain($"às {Now:HH:mm}");
    }

    [Fact]
    public async Task Ingresso_de_outro_evento_devolve_400()
    {
        var showA = await CreateEventAsync();
        var showB = await CreateEventAsync();
        var lote = await CreateLoteAsync(showA.Id);
        var code = (await CreatePaidOrderAsync(lote.Id)).Tickets[0].Code;

        var response = await CheckInAsync(showB.Id, code);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Ingresso_inexistente_devolve_404()
    {
        var ev = await CreateEventAsync();

        var response = await CheckInAsync(ev.Id, Guid.NewGuid());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Portaria_abre_30_minutos_antes_do_inicio()
    {
        var ev = await CreateEventAsync(start: Now.AddHours(2), end: Now.AddHours(5));
        var lote = await CreateLoteAsync(ev.Id);
        var code = (await CreatePaidOrderAsync(lote.Id)).Tickets[0].Code;

        var tooEarly = await CheckInAsync(ev.Id, code);
        tooEarly.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        Time.Advance(TimeSpan.FromMinutes(90)); // agora = início − 30 min
        var onTime = await CheckInAsync(ev.Id, code);
        onTime.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Check_in_depois_do_fim_do_evento_devolve_409()
    {
        var ev = await CreateEventAsync(start: Now.AddHours(-1), end: Now.AddHours(1));
        var lote = await CreateLoteAsync(ev.Id);
        var code = (await CreatePaidOrderAsync(lote.Id)).Tickets[0].Code;

        Time.Advance(TimeSpan.FromHours(2));
        var response = await CheckInAsync(ev.Id, code);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ReadProblemAsync(response)).Detail!.ShouldContain("check-in está encerrado");
    }

    [Fact]
    public async Task Leituras_simultaneas_do_mesmo_QR_registram_uma_unica_entrada()
    {
        var ev = await CreateEventAsync();
        var lote = await CreateLoteAsync(ev.Id);
        var code = (await CreatePaidOrderAsync(lote.Id)).Tickets[0].Code;

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => CheckInAsync(ev.Id, code)));

        responses.Count(r => r.StatusCode == HttpStatusCode.OK).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(9);
    }
}
