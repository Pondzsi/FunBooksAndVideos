using FunBooksAndVideos.Application.Orders;
using FunBooksAndVideos.Application.Orders.Processing;
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

        // Finds the domain event handlers in this assembly.
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
