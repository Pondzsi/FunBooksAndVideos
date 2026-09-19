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

        return services;
    }
}
