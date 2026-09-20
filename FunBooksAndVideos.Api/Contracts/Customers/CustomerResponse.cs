using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Api.Contracts.Customers;

// AccessibleCategories is everything the customer's memberships give them access to together.
public sealed record CustomerResponse(
    long Id,
    string Name,
    IReadOnlyList<MembershipResponse> Memberships,
    IReadOnlyList<ProductCategory> AccessibleCategories)
{
    public static CustomerResponse From(Customer customer)
    {
        return new CustomerResponse(
            customer.Id,
            customer.Name,
            customer.Memberships.Select(MembershipResponse.From).ToList(),
            Enum.GetValues<ProductCategory>().Where(customer.HasAccessTo).ToList());
    }
}
