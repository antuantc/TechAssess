namespace InterviewProblems.Senior;

/// <summary>
/// Senior — Advanced SQL (SQLite).
///
/// Each method returns a SQL query <b>string</b>. Runs against the same seeded
/// database as the other levels (<c>src/InterviewProblems/Data/schema.sql</c>).
/// These require window functions, CTEs, and correlated subqueries. As with the
/// Developer 2 SQL problems, only 'Completed' orders count toward revenue.
/// </summary>
public class AdvancedSqlProblems
{
    /// <summary>
    /// Rank customers by their total 'Completed' revenue (highest = rank 1).
    /// Return two columns — customer <c>Name</c> and their <c>Rank</c> — ordered
    /// by rank. Only include customers with completed orders.
    /// (Hint: <c>ROW_NUMBER() OVER (...)</c>.)
    /// Expected: (Bob, 1), (Dave, 2), (Alice, 3).
    /// </summary>
    public string CustomerRevenueRanking()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return a single value: the second-highest distinct product price.
    /// Expected: 25.
    /// </summary>
    public string SecondHighestProductPrice()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// For each calendar month, return the month ('YYYY-MM'), that month's
    /// 'Completed' revenue, and the running cumulative revenue through that
    /// month, ordered by month.
    /// (Hint: <c>SUM(...) OVER (ORDER BY month)</c>.)
    /// Expected: (2024-06, 70, 70), (2024-07, 250, 320).
    /// </summary>
    public string RunningMonthlyRevenue()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the <c>Id</c> of every 'Completed' order whose total value is
    /// greater than the average total value across all completed orders,
    /// ordered by order id.
    /// (Order total = SUM(Quantity * UnitPrice) for that order.)
    /// Expected: 3, 5.
    /// </summary>
    public string OrdersAboveAverageValue()
    {
        throw new NotImplementedException();
    }
}
