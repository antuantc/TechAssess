using InterviewProblems.Developer2;

namespace InterviewProblems.Tests.Developer2;

[Trait("Level", "Developer2")]
[Trait("Category", "SQL")]
public class SqlProblemsTests
{
    private readonly SqlProblems _sut = new();

    [Fact]
    public void CountActiveCustomers_returns_4()
    {
        var count = Convert.ToInt64(SqlTestDatabase.Scalar(_sut.CountActiveCustomers()));
        Assert.Equal(4, count);
    }

    [Fact]
    public void HardwareProductsByPriceDescending_orders_by_price()
    {
        var names = SqlTestDatabase.Column(_sut.HardwareProductsByPriceDescending())
            .Select(value => value?.ToString())
            .ToArray();

        Assert.Equal(new[] { "Gadget", "Widget", "Gizmo" }, names);
    }

    [Fact]
    public void RevenuePerCustomer_returns_totals_desc()
    {
        var rows = SqlTestDatabase.Rows(_sut.RevenuePerCustomer());

        Assert.Collection(rows,
            row => AssertNameAndAmount(row, "Bob", 160),
            row => AssertNameAndAmount(row, "Dave", 90),
            row => AssertNameAndAmount(row, "Alice", 70));
    }

    [Fact]
    public void CustomersWithNoOrders_returns_eve()
    {
        var names = SqlTestDatabase.Column(_sut.CustomersWithNoOrders())
            .Select(value => value?.ToString())
            .ToArray();

        Assert.Equal(new[] { "Eve" }, names);
    }

    [Fact]
    public void TopSellingProducts_returns_top_three_by_quantity()
    {
        var rows = SqlTestDatabase.Rows(_sut.TopSellingProducts());

        Assert.Collection(rows,
            row => AssertNameAndAmount(row, "Widget", 6),
            row => AssertNameAndAmount(row, "Gizmo", 5),
            row => AssertNameAndAmount(row, "eBook", 4));
    }

    [Fact]
    public void RevenueByMonth_groups_and_orders_by_month()
    {
        var rows = SqlTestDatabase.Rows(_sut.RevenueByMonth());

        Assert.Collection(rows,
            row => AssertNameAndAmount(row, "2024-06", 70),
            row => AssertNameAndAmount(row, "2024-07", 250));
    }

    private static void AssertNameAndAmount(object?[] row, string expectedName, double expectedAmount)
    {
        Assert.Equal(expectedName, row[0]?.ToString());
        Assert.Equal(expectedAmount, Convert.ToDouble(row[1]), precision: 2);
    }
}
