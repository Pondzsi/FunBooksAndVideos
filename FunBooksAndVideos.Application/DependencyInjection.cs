using FunBooksAndVideos.Application.Customers;
using FunBooksAndVideos.Application.Orders;
using FunBooksAndVideos.Application.Orders.Processing;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Application.Shipping;
using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Rules run in registration order. A new business rule is one new class plus one line here.
        services.AddScoped<IPurchaseOrderRule, ActivateMembershipRule>();
        services.AddScoped<IPurchaseOrderRule, GenerateShippingSlipRule>();
        services.AddScoped<PurchaseOrderProcessor>();
        services.AddScoped<PlacePurchaseOrder>();
        services.AddScoped<GetPurchaseOrder>();
        services.AddScoped<GetShippingSlip>();
        services.AddScoped<GetCustomer>();
        services.AddScoped<GetProduct>();
        services.AddScoped<GetProducts>();

        // Finds the domain event handlers in this assembly.
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
