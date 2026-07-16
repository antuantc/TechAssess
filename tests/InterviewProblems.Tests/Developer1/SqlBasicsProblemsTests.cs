using InterviewProblems.Developer1;

namespace InterviewProblems.Tests.Developer1;

[Trait("Level", "Developer1")]
[Trait("Category", "SQL")]
public class SqlBasicsProblemsTests
{
    private readonly SqlBasicsProblems _sut = new();

    [Fact]
    public void AllCustomerNamesAlphabetical_returns_sorted_names()
    {
        var names = SqlTestDatabase.Column(_sut.AllCustomerNamesAlphabetical())
            .Select(value => value?.ToString())
            .ToArray();

        Assert.Equal(new[] { "Alice", "Bob", "Carol", "Dave", "Eve" }, names);
    }

    [Fact]
    public void CustomersInPortland_returns_portland_customers()
    {
        var names = SqlTestDatabase.Column(_sut.CustomersInPortland())
            .Select(value => value?.ToString())
            .ToArray();

        Assert.Equal(new[] { "Bob", "Eve" }, names);
    }

    [Fact]
    public void ProductsCheaperThan20_returns_cheap_products_by_price()
    {
        var names = SqlTestDatabase.Column(_sut.ProductsCheaperThan20())
            .Select(value => value?.ToString())
            .ToArray();

        Assert.Equal(new[] { "Gizmo", "Widget", "eBook" }, names);
    }

    [Fact]
    public void TotalNumberOfOrders_returns_5()
    {
        var count = Convert.ToInt64(SqlTestDatabase.Scalar(_sut.TotalNumberOfOrders()));
        Assert.Equal(5, count);
    }
}
