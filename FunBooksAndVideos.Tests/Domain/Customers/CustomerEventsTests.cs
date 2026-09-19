using FunBooksAndVideos.Domain.Customers;

namespace FunBooksAndVideos.Tests.Domain.Customers;

public class CustomerEventsTests
{
    [Fact]
    public void Activating_a_membership_raises_MembershipActivated()
    {
        var customer = TestData.NewCustomer();

        customer.ActivateMembership(TestData.BookClub, TestData.Now);

        var domainEvent = Assert.Single(customer.DomainEvents);
        Assert.Equal(new MembershipActivated(customer.Id, TestData.BookClub.Id), domainEvent);
    }

    [Fact]
    public void Activating_the_same_membership_again_raises_no_second_event()
    {
        var customer = TestData.NewCustomer();

        customer.ActivateMembership(TestData.BookClub, TestData.Now);
        customer.ActivateMembership(TestData.BookClub, TestData.Now.AddDays(1));

        Assert.Single(customer.DomainEvents);
    }

    [Fact]
    public void Clearing_domain_events_removes_them()
    {
        var customer = TestData.NewCustomer();
        customer.ActivateMembership(TestData.BookClub, TestData.Now);

        customer.ClearDomainEvents();

        Assert.Empty(customer.DomainEvents);
    }
}
