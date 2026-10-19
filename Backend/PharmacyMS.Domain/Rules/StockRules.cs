namespace PharmacyMS.Domain.Rules;

public static class StockRules
{
    public static decimal LineTotal(int quantity, decimal unitPrice)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPrice < 0 || decimal.Round(unitPrice, 2) != unitPrice)
            throw new ArgumentOutOfRangeException(nameof(unitPrice));
        return checked(quantity * unitPrice);
    }

    public static decimal GrandTotal(IEnumerable<decimal> lineTotals, decimal discount)
    {
        var subtotal = lineTotals.Sum();
        if (discount < 0 || decimal.Round(discount, 2) != discount || discount > subtotal)
            throw new ArgumentOutOfRangeException(nameof(discount), "Discount must be between 0 and subtotal.");
        return subtotal - discount;
    }

    public static void EnsureSaleable(int stock, int quantity, DateOnly expiry, DateOnly today)
    {
        if (quantity <= 0 || quantity > stock) throw new InvalidOperationException("Insufficient stock.");
        if (expiry <= today) throw new InvalidOperationException("Expired medicine cannot be sold.");
    }
}
