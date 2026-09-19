namespace FunBooksAndVideos.Application.Orders.Processing;

public interface IPurchaseOrderRule
{
    void Apply(PurchaseOrderProcessingContext context);
}
