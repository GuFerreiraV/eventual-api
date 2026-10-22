using Eventual.Api.Data;
using Eventual.Api.Handlers;
using Eventual.Api.Services;
using Eventual.Api.Services.Payments;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Docker secrets: cada arquivo em /run/secrets vira uma chave de configuração
// (Database__Password -> Database:Password). Fora do container, a senha vem do user-secrets.
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

// Add services to the container.
builder.Services.AddControllers();

var connectionString = new NpgsqlConnectionStringBuilder(builder.Configuration.GetConnectionString("Eventual"))
{
    Password = builder.Configuration["Database:Password"]
}.ConnectionString;

builder.Services.AddDbContext<EventualDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Eventual API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Venda de ingressos de eventos: lotes com estoque, reserva com expiração, " +
            "pagamento (gateway de testes) e check-in por QR code.";
        return Task.CompletedTask;
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Em dev o schema acompanha as migrations sem passo manual.
    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<EventualDbContext>().Database.Migrate();
    }

    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Permite que o projeto de testes suba a API com WebApplicationFactory<Program>.
public partial class Program;
