using System.Net;
using System.Net.Http.Json;
using Eventual.Api.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace Eventual.Tests.Infrastructure;

// Cada teste ganha uma API nova (banco e relógio próprios) e monta o cenário só por HTTP.
public abstract class ApiTestBase : IDisposable
{
    protected readonly EventualApiFactory Factory = new();
    protected readonly HttpClient Client;

    protected ApiTestBase() => Client = Factory.CreateClient();

    protected FakeTimeProvider Time => Factory.Time;
    protected DateTime Now => Time.GetUtcNow().UtcDateTime;

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }

    // Por padrão o evento já começou e termina em 5 horas: dá para comprar e fazer check-in.
    protected async Task<EventDetailResponse> CreateEventAsync(DateTime? start = null, DateTime? end = null)
    {
        var response = await Client.PostAsJsonAsync("/events", new
        {
            name = "Show de Rock",
            description = "Teste",
            local = "Arena",
            start = start ?? Now.AddHours(-1),
            end = end ?? Now.AddHours(5)
        });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<EventDetailResponse>())!;
    }

    protected async Task<LoteResponse> CreateLoteAsync(int eventId, int quantity = 10, decimal price = 50m, string name = "Pista")
    {
        var response = await Client.PostAsJsonAsync($"/events/{eventId}/lotes", new { name, price, quantity });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<LoteResponse>())!;
    }

    protected Task<HttpResponseMessage> PostOrderAsync(int loteId, int quantity) =>
        Client.PostAsJsonAsync($"/lotes/{loteId}/orders", new
        {
            quantity,
            buyerName = "Ana Souza",
            buyerEmail = "ana@exemplo.com"
        });

    protected async Task<OrderResponse> CreateOrderAsync(int loteId, int quantity = 1)
    {
        var response = await PostOrderAsync(loteId, quantity);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }

    protected Task<HttpResponseMessage> PayAsync(int orderId, string token = "tok_approved") =>
        Client.PostAsJsonAsync($"/orders/{orderId}/payment", new { paymentToken = token });

    protected async Task<OrderResponse> CreatePaidOrderAsync(int loteId, int quantity = 1)
    {
        var order = await CreateOrderAsync(loteId, quantity);
        var response = await PayAsync(order.Id);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }

    protected Task<HttpResponseMessage> CheckInAsync(int eventId, Guid code) =>
        Client.PostAsJsonAsync($"/events/{eventId}/check-in", new { code });

    protected async Task<OrderResponse> GetOrderAsync(int id) =>
        (await Client.GetFromJsonAsync<OrderResponse>($"/orders/{id}"))!;

    protected async Task<LoteResponse> GetLoteAsync(int id) =>
        (await Client.GetFromJsonAsync<LoteResponse>($"/lotes/{id}"))!;

    protected static async Task<ProblemDetails> ReadProblemAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
}
