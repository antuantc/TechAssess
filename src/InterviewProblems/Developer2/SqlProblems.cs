namespace InterviewProblems.Developer2;

/// <summary>
/// Developer 2 — SQL problems (SQLite).
///
/// Each method returns a SQL query <b>string</b>. The tests execute your query
/// against a freshly seeded in-memory SQLite database and compare the results
/// against the expected output.
///
/// Schema (see <c>src/InterviewProblems/Data/schema.sql</c> for the
/// full definition and <c>seed.sql</c> for the sample data):
///
///   Customers(Id, Name, Email, City, IsActive, CreatedAt)
///   Products(Id, Name, Category, Price)
///   Orders(Id, CustomerId, OrderDate, Status)         -- Status: 'Completed' | 'Cancelled'
///   OrderItems(Id, OrderId, ProductId, Quantity, UnitPrice)
///
/// Revenue for a line item = Quantity * UnitPrice.
/// Unless a problem says otherwise, only 'Completed' orders count toward revenue.
/// </summary>
public class SqlProblems
{
    /// <summary>
    /// Return a single row with a single column: the number of customers whose
    /// <c>IsActive</c> flag is 1.
    /// Expected result: 4.
    /// </summary>
    public string CountActiveCustomers()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the <c>Name</c> of every product in the 'Hardware' category,
    /// ordered by <c>Price</c> from highest to lowest.
    /// Expected result: Gadget, Widget, Gizmo.
    /// </summary>
    public string HardwareProductsByPriceDescending()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// For every customer who has at least one 'Completed' order, return two
    /// columns — the customer <c>Name</c> and their total revenue — ordered by
    /// revenue from highest to lowest.
    /// (Revenue = SUM(Quantity * UnitPrice) across their completed orders.)
    /// Expected: (Bob, 160), (Dave, 90), (Alice, 70).
    /// </summary>
    public string RevenuePerCustomer()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the <c>Name</c> of every customer who has never placed an order
    /// (of any status).
    /// Expected result: Eve.
    /// </summary>
    public string CustomersWithNoOrders()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the top 3 best-selling products across 'Completed' orders as two
    /// columns — product <c>Name</c> and total quantity sold — ordered by total
    /// quantity descending, breaking ties by product name ascending.
    /// Expected: (Widget, 6), (Gizmo, 5), (eBook, 4).
    /// </summary>
    public string TopSellingProducts()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return total revenue per calendar month across 'Completed' orders as two
    /// columns — the month as 'YYYY-MM' and the total revenue — ordered by month
    /// ascending. (Hint: SQLite <c>strftime('%Y-%m', OrderDate)</c>.)
    /// Expected: (2024-06, 70), (2024-07, 250).
    /// </summary>
    public string RevenueByMonth()
    {
        throw new NotImplementedException();
    }
}
