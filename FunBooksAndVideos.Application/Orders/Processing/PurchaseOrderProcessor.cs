namespace FunBooksAndVideos.Application.Orders.Processing;

public sealed class PurchaseOrderProcessor
{
    private readonly IReadOnlyList<IPurchaseOrderRule> _rules;

    public PurchaseOrderProcessor(IEnumerable<IPurchaseOrderRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        _rules = rules.ToList();
    }

    public void Process(PurchaseOrderProcessingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var rule in _rules)
        {
            rule.Apply(context);
        }
    }
}
