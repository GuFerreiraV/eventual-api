using Eventual.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace Eventual.Tests.Infrastructure;

// Sobe a API inteira em memória com um relógio controlável e um banco InMemory exclusivo.
public class EventualApiFactory : WebApplicationFactory<Program>
{
    public static readonly DateTimeOffset StartTime = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    public FakeTimeProvider Time { get; } = new(StartTime);

    private readonly string _databaseName = $"eventual-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);

            services.RemoveAll<DbContextOptions<EventualDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<EventualDbContext>>();
            services.AddDbContext<EventualDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
