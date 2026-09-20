using FunBooksAndVideos.Application;
using FunBooksAndVideos.Infrastructure;
using FunBooksAndVideos.Tests.Application;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;

namespace FunBooksAndVideos.Tests.Infrastructure;

// One SQL Server container for all the integration tests. Each test gets its own database inside it.
public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public Task InitializeAsync()
    {
        // Skipped tests never need the container.
        return SqlServerFactAttribute.DockerIsRunning ? _container.StartAsync() : Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    // The application's own services on a new database that is already migrated and seeded, like the API at startup.
    public async Task<ServiceProvider> CreateSeededDatabaseAsync(LogSink? sink = null)
    {
        var connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = $"test_{Guid.NewGuid():N}",
        }.ConnectionString;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = connectionString })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(sink ?? new LogSink());
        services.AddSingleton(typeof(ILogger<>), typeof(ListLogger<>));
        services.AddApplication();
        services.AddInfrastructure(configuration);

        var provider = services.BuildServiceProvider();
        await provider.InitializeDatabaseAsync();

        return provider;
    }
}
