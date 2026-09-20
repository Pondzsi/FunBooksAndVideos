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
        // Retrying also covers SQL Server still starting up when the API boots.
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("The connection string 'Default' is not configured.");

            options.UseSqlServer(connectionString, sqlServer => sqlServer.EnableRetryOnFailure());
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
}
