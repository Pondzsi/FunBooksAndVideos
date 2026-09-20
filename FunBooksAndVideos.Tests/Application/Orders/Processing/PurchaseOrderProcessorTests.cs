using FunBooksAndVideos.Application.Orders.Processing;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Tests.Application.Orders.Processing;

public class PurchaseOrderProcessorTests
{
    private static PurchaseOrderProcessingContext PdfExampleContext()
    {
        return new PurchaseOrderProcessingContext(TestData.PdfExampleOrder(), TestData.NewCustomer(), TestData.Now);
    }

    [Fact]
    public void Applies_both_business_rules_to_the_PDF_example_order()
    {
        var processor = new PurchaseOrderProcessor([new ActivateMembershipRule(), new GenerateShippingSlipRule()]);
        var context = PdfExampleContext();

        processor.Process(context);

        Assert.True(context.Customer.HasAccessTo(ProductCategory.Book));
        Assert.False(context.Customer.HasAccessTo(ProductCategory.Video));
        var slip = Assert.IsType<FunBooksAndVideos.Domain.Shipping.ShippingSlip>(context.ShippingSlip);
        Assert.Equal(TestData.GirlOnTheTrain.Id, Assert.Single(slip.Items).ProductId);
    }

    [Fact]
    public void Applies_every_registered_rule_in_registration_order()
    {
        var log = new List<string>();
        var processor = new PurchaseOrderProcessor(
            [new RecordingRule("first", log), new RecordingRule("second", log), new RecordingRule("third", log)]);

        processor.Process(PdfExampleContext());

        Assert.Equal(new[] { "first", "second", "third" }, log);
    }

    [Fact]
    public void Runs_a_custom_rule_alongside_the_built_in_ones_without_changing_the_processor()
    {
        var log = new List<string>();
        var processor = new PurchaseOrderProcessor(
            [new ActivateMembershipRule(), new RecordingRule("custom", log), new GenerateShippingSlipRule()]);
        var context = PdfExampleContext();

        processor.Process(context);

        Assert.Equal(new[] { "custom" }, log);
        Assert.NotEmpty(context.Customer.Memberships);
        Assert.NotNull(context.ShippingSlip);
    }

    private sealed class RecordingRule(string name, List<string> log) : IPurchaseOrderRule
    {
        public void Apply(PurchaseOrderProcessingContext context)
        {
            log.Add(name);
        }
    }
}
