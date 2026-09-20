using FunBooksAndVideos.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Tests.Infrastructure;

// Needs no Docker: it points at a port where nothing is listening.
public class DatabaseInitializerTests
{
    [Fact]
    public async Task An_unreachable_database_fails_with_a_readable_message_that_hides_the_password()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Server=127.0.0.1,1;Database=Missing;User Id=sa;Password=SuperSecret1!;TrustServerCertificate=True;Connect Timeout=1",
                ["Database:MaxRetryCount"] = "0", // fail at once instead of waiting through the usual retries
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        await using var provider = services.BuildServiceProvider();

        var exception = await Assert.ThrowsAsync<DatabaseInitializationException>(() => provider.InitializeDatabaseAsync());

        Assert.Contains("127.0.0.1,1", exception.Message);
        Assert.Contains("docker compose up -d --wait", exception.Message);
        Assert.DoesNotContain("SuperSecret1!", exception.Message);
    }
}
