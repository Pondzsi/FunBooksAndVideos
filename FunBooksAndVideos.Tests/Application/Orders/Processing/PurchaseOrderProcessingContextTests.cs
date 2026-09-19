using FunBooksAndVideos.Application.Orders.Processing;
using FunBooksAndVideos.Domain.Customers;

namespace FunBooksAndVideos.Tests.Application.Orders.Processing;

public class PurchaseOrderProcessingContextTests
{
    [Fact]
    public void Customer_must_be_the_one_who_placed_the_order()
    {
        var someoneElse = new Customer(999, "Someone Else");

        Assert.Throws<ArgumentException>(
            () => new PurchaseOrderProcessingContext(TestData.PdfExampleOrder(), someoneElse, TestData.Now));
    }
}
