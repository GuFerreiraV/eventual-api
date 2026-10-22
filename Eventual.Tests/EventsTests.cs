using System.Net;
using System.Net.Http.Json;
using Eventual.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Eventual.Tests;

public class EventsTests : ApiTestBase
{
    [Fact]
    public async Task Criar_evento_devolve_201_com_location()
    {
        var response = await Client.PostAsJsonAsync("/events", new
        {
            name = "Show",
            local = "Arena",
            start = Now.AddDays(1),
            end = Now.AddDays(1).AddHours(3)
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location!.AbsolutePath.ShouldBe("/events/1");
    }

    [Fact]
    public async Task Criar_evento_com_termino_antes_do_inicio_devolve_400()
    {
        var response = await Client.PostAsJsonAsync("/events", new
        {
            name = "Show",
            local = "Arena",
            start = Now.AddDays(1),
            end = Now.AddDays(1).AddHours(-3)
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.ShouldContainKey("End");
    }

    [Fact]
    public async Task Evento_inexistente_devolve_404()
    {
        var response = await Client.GetAsync("/events/99");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadProblemAsync(response)).Detail.ShouldBe("Evento 99 não encontrado.");
    }

    [Fact]
    public async Task Detalhe_do_evento_traz_os_lotes()
    {
        var ev = await CreateEventAsync();
        await CreateLoteAsync(ev.Id, name: "Pista");
        await CreateLoteAsync(ev.Id, name: "Camarote");

        var detail = await Client.GetFromJsonAsync<Eventual.Api.Models.Dtos.EventDetailResponse>($"/events/{ev.Id}");

        detail!.Lotes.Select(l => l.Name).ShouldBe(["Pista", "Camarote"]);
    }
}
