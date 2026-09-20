using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FunBooksAndVideos.Tests.Api;

// The whole API in memory, on a database of its own. Startup migrates and seeds it, as it does for real.
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    // SQL Server in a container, especially an emulated one on Apple Silicon, can take well over the default 30 seconds to create a database under load.
    public const string SlowSqlServerCommandTimeoutSeconds = "120";

    private readonly string _connectionString;

    public ApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _connectionString,
                ["Database:CommandTimeoutSeconds"] = SlowSqlServerCommandTimeoutSeconds,
            }));
    }

    public static Task<HttpResponseMessage> PlaceOrderAsync(HttpClient client, long customerId, params long[] productIds)
    {
        return client.PostAsJsonAsync("/api/v1/purchase-orders", new { customerId, productIds }, Json);
    }

    public static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }
}
