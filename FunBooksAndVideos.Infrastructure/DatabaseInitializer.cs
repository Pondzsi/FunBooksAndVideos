using FunBooksAndVideos.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Infrastructure;

public static class DatabaseInitializer
{
    // Applies pending migrations, then seeds an empty database.
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is SqlException or RetryLimitExceededException)
        {
            throw new DatabaseInitializationException(Describe(context), exception);
        }

        await scope.ServiceProvider.GetRequiredService<Seeder>().SeedAsync(cancellationToken);
    }

    // Server and database name only, never the credentials.
    private static string Describe(AppDbContext context)
    {
        var connection = new SqlConnectionStringBuilder(context.Database.GetConnectionString());

        return $"'{connection.DataSource}' (database '{connection.InitialCatalog}')";
    }
}
