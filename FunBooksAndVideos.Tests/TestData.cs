using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Tests;

internal static class TestData
{
    // The PDF only gives the total (48.50) for its example order, so these prices are illustrative.
    public static readonly Product FirstAidVideo = new DigitalProduct(1, "Comprehensive First Aid Training", 24.00m, ProductCategory.Video);
    public static readonly Product GirlOnTheTrain = new PhysicalProduct(2, "The Girl on the train", 9.50m, ProductCategory.Book);
    public static readonly MembershipProduct BookClub = new(3, "Book Club Membership", 15.00m, [ProductCategory.Book]);
    public static readonly MembershipProduct VideoClub = new(4, "Video Club Membership", 15.00m, [ProductCategory.Video]);
    public static readonly MembershipProduct Premium = new(5, "Premium Membership", 25.00m, [ProductCategory.Book, ProductCategory.Video]);

    public static readonly DateTimeOffset Now = new(2026, 9, 19, 12, 0, 0, TimeSpan.Zero);

    public static Customer NewCustomer() => new(4567890, "Test Customer");

    public static PurchaseOrder PdfExampleOrder() => new(3344656, 4567890, [FirstAidVideo, GirlOnTheTrain, BookClub]);
}
