using Testcontainers.PostgreSql;

namespace Eventual.Tests.Infrastructure;

// Um único Postgres descartável para a execução inteira de testes.
// O Testcontainers remove o container sozinho quando o processo de testes termina.
public static class PostgresContainer
{
    private static readonly Lazy<PostgreSqlContainer> Instance = new(() =>
    {
        var container = new PostgreSqlBuilder("postgres:17").Build();
        container.StartAsync().GetAwaiter().GetResult();
        return container;
    });

    // Cada chamada aponta para um banco novo dentro do mesmo container (criado pela migration).
    public static string NewDatabaseConnectionString() =>
        new Npgsql.NpgsqlConnectionStringBuilder(Instance.Value.GetConnectionString())
        {
            Database = $"eventual_tests_{Guid.NewGuid():N}"
        }.ConnectionString;
}
