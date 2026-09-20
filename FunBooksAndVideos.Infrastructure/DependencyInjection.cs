using FunBooksAndVideos.Application.Common;
using FunBooksAndVideos.Application.Customers;
using FunBooksAndVideos.Application.Orders;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Application.Shipping;
using FunBooksAndVideos.Infrastructure.Persistence;
using FunBooksAndVideos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Read when the context is first created, so configuration added after registration (such as a test host's) still applies.
        // Retrying covers SQL Server still starting up when the API boots. Both settings can be raised in configuration for a slow
        // or emulated SQL Server (the tests do): Database:MaxRetryCount and Database:CommandTimeoutSeconds.
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("The connection string 'Default' is not configured.");

            options.UseSqlServer(connectionString, sqlServer =>
            {
                sqlServer.EnableRetryOnFailure(ReadInt(configuration, "Database:MaxRetryCount", 6), TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                sqlServer.CommandTimeout(ReadInt(configuration, "Database:CommandTimeoutSeconds", 30));
            });
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IShippingSlipRepository, ShippingSlipRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<Seeder>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }

    private static int ReadInt(IConfiguration configuration, string key, int fallback)
    {
        return int.TryParse(configuration[key], out var value) ? value : fallback;
    }
}
