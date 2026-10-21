using PharmacyMS.Domain.Rules;
using Xunit;

namespace PharmacyMS.Domain.Tests;

public class StockRulesTests
{
    [Fact]
    public void LineTotal_ShouldCalculateCorrectly()
    {
        var result = StockRules.LineTotal(3, 12.50m);

        Assert.Equal(37.50m, result);
    }

    [Fact]
    public void GrandTotal_ShouldApplyDiscount()
    {
        var total = StockRules.LineTotal(3, 12.50m);

        var result = StockRules.GrandTotal(
            [total],
            5m
        );

        Assert.Equal(32.50m, result);
    }

    [Fact]
    public void EnsureSaleable_ShouldRejectInsufficientStock()
    {
        Assert.Throws<InvalidOperationException>(() =>
            StockRules.EnsureSaleable(
                2,
                3,
                new DateOnly(2030, 1, 1),
                new DateOnly(2026, 10, 17)
            )
        );
    }

    [Fact]
    public void EnsureSaleable_ShouldRejectExpiredStock()
    {
        Assert.Throws<InvalidOperationException>(() =>
            StockRules.EnsureSaleable(
                3,
                1,
                new DateOnly(2026, 10, 16),
                new DateOnly(2026, 10, 17)
            )
        );
    }
}