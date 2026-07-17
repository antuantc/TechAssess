using InterviewProblems.Infrastructure;

namespace InterviewProblems.Tests.Common;

public sealed class SqlQueryPolicyTests
{
    [Theory]
    [InlineData("INSERT INTO Customers (Name) VALUES ('attacker');")]
    [InlineData("PRAGMA writable_schema = 1;")]
    [InlineData("ATTACH DATABASE ':memory:' AS other;")]
    [InlineData("SELECT 1; DROP TABLE Customers;")]
    public void Rejects_non_read_only_or_multiple_statements(string sql)
    {
        Assert.False(SqlQueryPolicy.IsReadOnlySingleQuery(sql));
    }

    [Theory]
    [InlineData("SELECT Name FROM Customers;")]
    [InlineData("WITH names AS (SELECT Name FROM Customers) SELECT Name FROM names;")]
    [InlineData("EXPLAIN SELECT Name FROM Customers;")]
    public void Allows_read_only_queries(string sql)
    {
        Assert.True(SqlQueryPolicy.IsReadOnlySingleQuery(sql));
    }

    [Fact]
    public void Query_rejects_mutation_at_database_boundary()
    {
        using var connection = InterviewDatabase.CreateSeededConnection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            InterviewDatabase.Query(connection, "DELETE FROM Customers;"));

        Assert.Contains("read-only", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}