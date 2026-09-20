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

    private readonly string _connectionString;

    public ApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = _connectionString }));
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
