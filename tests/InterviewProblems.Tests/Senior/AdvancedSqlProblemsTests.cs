using InterviewProblems.Senior;

namespace InterviewProblems.Tests.Senior;

[Trait("Level", "Senior")]
[Trait("Category", "SQL")]
public class AdvancedSqlProblemsTests
{
    private readonly AdvancedSqlProblems _sut = new();

    [Fact]
    public void CustomerRevenueRanking_ranks_by_revenue()
    {
        var rows = SqlTestDatabase.Rows(_sut.CustomerRevenueRanking());

        Assert.Collection(rows,
            row => AssertNameAndRank(row, "Bob", 1),
            row => AssertNameAndRank(row, "Dave", 2),
            row => AssertNameAndRank(row, "Alice", 3));
    }

    [Fact]
    public void SecondHighestProductPrice_returns_25()
    {
        var price = Convert.ToDouble(SqlTestDatabase.Scalar(_sut.SecondHighestProductPrice()));
        Assert.Equal(25d, price, precision: 2);
    }

    [Fact]
    public void RunningMonthlyRevenue_returns_cumulative_totals()
    {
        var rows = SqlTestDatabase.Rows(_sut.RunningMonthlyRevenue());

        Assert.Collection(rows,
            row => AssertMonthRevenueRunning(row, "2024-06", 70, 70),
            row => AssertMonthRevenueRunning(row, "2024-07", 250, 320));
    }

    [Fact]
    public void OrdersAboveAverageValue_returns_order_ids()
    {
        var ids = SqlTestDatabase.Column(_sut.OrdersAboveAverageValue())
            .Select(Convert.ToInt64)
            .ToArray();

        Assert.Equal(new[] { 3L, 5L }, ids);
    }

    private static void AssertNameAndRank(object?[] row, string expectedName, long expectedRank)
    {
        Assert.Equal(expectedName, row[0]?.ToString());
        Assert.Equal(expectedRank, Convert.ToInt64(row[1]));
    }

    private static void AssertMonthRevenueRunning(
        object?[] row, string expectedMonth, double expectedRevenue, double expectedRunning)
    {
        Assert.Equal(expectedMonth, row[0]?.ToString());
        Assert.Equal(expectedRevenue, Convert.ToDouble(row[1]), precision: 2);
        Assert.Equal(expectedRunning, Convert.ToDouble(row[2]), precision: 2);
    }
}
