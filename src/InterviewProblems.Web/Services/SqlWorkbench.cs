using System.Globalization;
using InterviewProblems.Infrastructure;
using Microsoft.Data.Sqlite;

namespace InterviewProblems.Web.Services;

/// <summary>
/// One SQL exercise: a natural-language prompt plus the reference query used to
/// grade the candidate's answer. The candidate writes raw SQL (sql-practice.com
/// style) rather than a C# method that returns a query string.
/// </summary>
public sealed record SqlQuestion(
    string Id,
    string Level,
    string Title,
    string Prompt,
    bool Ordered,
    string ReferenceSql,
    string StarterSql);

/// <summary>A column of a schema table, shown in the schema panel.</summary>
public sealed record SchemaColumn(string Name, string Type);

/// <summary>A table in the interview database, shown in the schema panel.</summary>
public sealed record SchemaTable(string Name, IReadOnlyList<SchemaColumn> Columns);

/// <summary>
/// The outcome of running a candidate's SQL: either an execution error, or the
/// returned grid plus whether it matches the reference result set.
/// </summary>
public sealed class SqlRunResult
{
    /// <summary>True when the candidate SQL executed without a database error.</summary>
    public bool Executed { get; init; }

    /// <summary>The database error message when <see cref="Executed"/> is false.</summary>
    public string? Error { get; init; }

    public IReadOnlyList<string> Columns { get; init; } = Array.Empty<string>();

    public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } =
        Array.Empty<IReadOnlyList<string>>();

    /// <summary>True when the result set matches the reference solution.</summary>
    public bool Correct { get; init; }

    /// <summary>Row count the reference solution returns (for the feedback line).</summary>
    public int ExpectedRowCount { get; init; }
}

/// <summary>
/// Runs candidate SQL against a freshly seeded in-memory SQLite database and
/// grades it by comparing the result set against the reference solution's
/// result set. The database is rebuilt and disposed on every run, so arbitrary
/// candidate SQL can never affect the host or another candidate.
/// </summary>
public sealed class SqlWorkbench
{
    private const char CellSeparator = '\u001f';
    private static readonly HashSet<string> ForbiddenKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "ALTER",
        "ATTACH",
        "CREATE",
        "DELETE",
        "DETACH",
        "DROP",
        "INSERT",
        "PRAGMA",
        "REINDEX",
        "RELEASE",
        "REPLACE",
        "ROLLBACK",
        "SAVEPOINT",
        "UPDATE",
        "VACUUM",
    };

    private readonly IReadOnlyList<SqlQuestion> _questions = BuildQuestions();
    private readonly Lazy<IReadOnlyList<SchemaTable>> _schema;

    public SqlWorkbench()
    {
        _schema = new Lazy<IReadOnlyList<SchemaTable>>(LoadSchema);
    }

    public IReadOnlyList<SqlQuestion> Questions => _questions;

    public IReadOnlyList<SchemaTable> Schema => _schema.Value;

    public IEnumerable<SqlQuestion> ForLevel(string level) =>
        _questions.Where(q => q.Level == level);

    public SqlQuestion? Find(string id) =>
        _questions.FirstOrDefault(q => q.Id == id);

    /// <summary>
    /// Execute <paramref name="candidateSql"/> and compare its result set to the
    /// question's reference solution.
    /// </summary>
    public SqlRunResult Run(string questionId, string candidateSql)
    {
        var question = Find(questionId)
            ?? throw new ArgumentException($"Unknown SQL question '{questionId}'.", nameof(questionId));

        if (string.IsNullOrWhiteSpace(candidateSql))
        {
            return new SqlRunResult { Executed = false, Error = "Write a query, then run it." };
        }

        var validationError = ValidateReadOnlyQuery(candidateSql);
        if (validationError is not null)
        {
            return new SqlRunResult { Executed = false, Error = validationError };
        }

        QueryResult candidate;
        try
        {
            using var connection = InterviewDatabase.CreateSeededConnection();
            candidate = InterviewDatabase.Query(connection, candidateSql);
        }
        catch (SqliteException ex)
        {
            return new SqlRunResult { Executed = false, Error = ex.Message };
        }
        catch (Exception ex)
        {
            return new SqlRunResult { Executed = false, Error = ex.Message };
        }

        QueryResult expected;
        using (var refConnection = InterviewDatabase.CreateSeededConnection())
        {
            expected = InterviewDatabase.Query(refConnection, question.ReferenceSql);
        }

        return new SqlRunResult
        {
            Executed = true,
            Columns = candidate.Columns,
            Rows = candidate.Rows.Select(DisplayRow).ToList(),
            Correct = ResultsMatch(candidate, expected, question.Ordered),
            ExpectedRowCount = expected.Rows.Count,
        };
    }

    private static string? ValidateReadOnlyQuery(string sql)
    {
        var firstKeyword = string.Empty;
        var statementEnded = false;

        for (var index = 0; index < sql.Length;)
        {
            if (char.IsWhiteSpace(sql[index]))
            {
                index++;
                continue;
            }

            if (sql[index] == '-' && index + 1 < sql.Length && sql[index + 1] == '-')
            {
                index += 2;
                while (index < sql.Length && sql[index] != '\n')
                {
                    index++;
                }

                continue;
            }

            if (sql[index] == '/' && index + 1 < sql.Length && sql[index + 1] == '*')
            {
                var commentEnd = sql.IndexOf("*/", index + 2, StringComparison.Ordinal);
                if (commentEnd < 0)
                {
                    return "The query contains an unterminated comment.";
                }

                index = commentEnd + 2;
                continue;
            }

            if (sql[index] is '\'' or '"' or '`')
            {
                var quote = sql[index++];
                var closed = false;
                while (index < sql.Length)
                {
                    if (sql[index] != quote)
                    {
                        index++;
                        continue;
                    }

                    if (index + 1 < sql.Length && sql[index + 1] == quote)
                    {
                        index += 2;
                        continue;
                    }

                    index++;
                    closed = true;
                    break;
                }

                if (!closed)
                {
                    return "The query contains an unterminated quoted value.";
                }

                continue;
            }

            if (sql[index] == ';')
            {
                if (statementEnded)
                {
                    return "Only one SQL statement is allowed.";
                }

                statementEnded = true;
                index++;
                continue;
            }

            if (statementEnded)
            {
                return "Only one SQL statement is allowed.";
            }

            if (char.IsLetter(sql[index]) || sql[index] == '_')
            {
                var wordStart = index++;
                while (index < sql.Length && (char.IsLetterOrDigit(sql[index]) || sql[index] == '_'))
                {
                    index++;
                }

                var keyword = sql[wordStart..index];
                firstKeyword = firstKeyword.Length == 0 ? keyword : firstKeyword;
                if (ForbiddenKeywords.Contains(keyword))
                {
                    return "Only read-only SELECT queries are allowed.";
                }

                continue;
            }

            index++;
        }

        return firstKeyword.Equals("SELECT", StringComparison.OrdinalIgnoreCase)
            || firstKeyword.Equals("WITH", StringComparison.OrdinalIgnoreCase)
            ? null
            : "Only read-only SELECT queries are allowed.";
    }

    private static IReadOnlyList<string> DisplayRow(object?[] row) =>
        row.Select(Display).ToList();

    private static bool ResultsMatch(QueryResult candidate, QueryResult expected, bool ordered)
    {
        var candidateKeys = candidate.Rows.Select(RowKey).ToList();
        var expectedKeys = expected.Rows.Select(RowKey).ToList();

        if (candidateKeys.Count != expectedKeys.Count)
        {
            return false;
        }

        if (ordered)
        {
            return candidateKeys.SequenceEqual(expectedKeys);
        }

        candidateKeys.Sort(StringComparer.Ordinal);
        expectedKeys.Sort(StringComparer.Ordinal);
        return candidateKeys.SequenceEqual(expectedKeys);
    }

    private static string RowKey(object?[] row) =>
        string.Join(CellSeparator, row.Select(Display));

    /// <summary>
    /// Normalize a cell for display and comparison. Integer-valued reals collapse
    /// so <c>160.0</c> and <c>160</c> compare equal (SUM returns a REAL in SQLite).
    /// </summary>
    private static string Display(object? value) => value switch
    {
        null => "NULL",
        double d => FormatNumber(d),
        float f => FormatNumber(f),
        decimal m => FormatNumber((double)m),
        _ => value.ToString() ?? string.Empty,
    };

    private static string FormatNumber(double d) =>
        !double.IsInfinity(d) && !double.IsNaN(d) && d == Math.Floor(d)
            ? ((long)d).ToString(CultureInfo.InvariantCulture)
            : d.ToString(CultureInfo.InvariantCulture);

    private static IReadOnlyList<SchemaTable> LoadSchema()
    {
        using var connection = InterviewDatabase.CreateSeededConnection();

        var tableNames = InterviewDatabase
            .Query(connection, "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;")
            .Rows
            .Select(r => r[0]?.ToString() ?? string.Empty)
            .Where(n => n.Length > 0)
            .ToList();

        var tables = new List<SchemaTable>();
        foreach (var name in tableNames)
        {
            var info = ReadSchemaInfo(connection, name);
            // PRAGMA table_info columns: cid, name, type, notnull, dflt_value, pk
            var columns = info.Rows
                .Select(r => new SchemaColumn(
                    r[1]?.ToString() ?? string.Empty,
                    r[2]?.ToString() ?? string.Empty))
                .ToList();
            tables.Add(new SchemaTable(name, columns));
        }

        return tables;
    }

    private static QueryResult ReadSchemaInfo(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";

        using var reader = command.ExecuteReader();
        var columns = new string[reader.FieldCount];
        for (var index = 0; index < reader.FieldCount; index++)
        {
            columns[index] = reader.GetName(index);
        }

        var rows = new List<object?[]>();
        while (reader.Read())
        {
            var values = new object?[reader.FieldCount];
            for (var index = 0; index < reader.FieldCount; index++)
            {
                values[index] = reader.IsDBNull(index) ? null : reader.GetValue(index);
            }

            rows.Add(values);
        }

        return new QueryResult(columns, rows);
    }

    private static IReadOnlyList<SqlQuestion> BuildQuestions()
    {
        const string starter = "-- Write your query here\n";

        return new List<SqlQuestion>
        {
            // -------------------- Developer 1 --------------------
            new("Developer1.AllCustomerNamesAlphabetical", "Developer1",
                "All Customer Names Alphabetical",
                "Return the Name of every customer, ordered alphabetically.",
                Ordered: true,
                "SELECT Name FROM Customers ORDER BY Name;",
                starter),

            new("Developer1.CustomersInPortland", "Developer1",
                "Customers In Portland",
                "Return the Name of every customer in the city 'Portland', ordered alphabetically.",
                Ordered: true,
                "SELECT Name FROM Customers WHERE City = 'Portland' ORDER BY Name;",
                starter),

            new("Developer1.ProductsCheaperThan20", "Developer1",
                "Products Cheaper Than 20",
                "Return the Name of every product priced below 20, ordered by price from lowest to highest.",
                Ordered: true,
                "SELECT Name FROM Products WHERE Price < 20 ORDER BY Price;",
                starter),

            new("Developer1.TotalNumberOfOrders", "Developer1",
                "Total Number Of Orders",
                "Return a single value: the total number of orders in the Orders table.",
                Ordered: false,
                "SELECT COUNT(*) FROM Orders;",
                starter),

            // -------------------- Developer 2 --------------------
            new("Developer2.CountActiveCustomers", "Developer2",
                "Count Active Customers",
                "Return the number of customers whose IsActive flag is 1.",
                Ordered: false,
                "SELECT COUNT(*) FROM Customers WHERE IsActive = 1;",
                starter),

            new("Developer2.HardwareProductsByPriceDescending", "Developer2",
                "Hardware Products By Price Descending",
                "Return the Name of every product in the 'Hardware' category, ordered by Price from highest to lowest.",
                Ordered: true,
                "SELECT Name FROM Products WHERE Category = 'Hardware' ORDER BY Price DESC;",
                starter),

            new("Developer2.RevenuePerCustomer", "Developer2",
                "Revenue Per Customer",
                "For every customer with at least one 'Completed' order, return the customer Name and their "
                + "total revenue (SUM of Quantity * UnitPrice), ordered by revenue from highest to lowest.",
                Ordered: true,
                "SELECT c.Name, SUM(oi.Quantity * oi.UnitPrice) AS Revenue\n"
                + "FROM Customers c\n"
                + "JOIN Orders o ON o.CustomerId = c.Id AND o.Status = 'Completed'\n"
                + "JOIN OrderItems oi ON oi.OrderId = o.Id\n"
                + "GROUP BY c.Id, c.Name\n"
                + "ORDER BY Revenue DESC;",
                starter),

            new("Developer2.CustomersWithNoOrders", "Developer2",
                "Customers With No Orders",
                "Return the Name of every customer who has never placed an order (of any status).",
                Ordered: false,
                "SELECT Name FROM Customers WHERE Id NOT IN (SELECT CustomerId FROM Orders);",
                starter),

            new("Developer2.TopSellingProducts", "Developer2",
                "Top Selling Products",
                "Return the top 3 best-selling products across 'Completed' orders as the product Name and total "
                + "quantity sold, ordered by total quantity descending, breaking ties by product name ascending.",
                Ordered: true,
                "SELECT p.Name, SUM(oi.Quantity) AS TotalQuantity\n"
                + "FROM OrderItems oi\n"
                + "JOIN Orders o ON o.Id = oi.OrderId AND o.Status = 'Completed'\n"
                + "JOIN Products p ON p.Id = oi.ProductId\n"
                + "GROUP BY p.Id, p.Name\n"
                + "ORDER BY TotalQuantity DESC, p.Name ASC\n"
                + "LIMIT 3;",
                starter),

            new("Developer2.RevenueByMonth", "Developer2",
                "Revenue By Month",
                "Return total revenue per calendar month across 'Completed' orders as the month ('YYYY-MM') and "
                + "the total revenue, ordered by month ascending. (Hint: strftime('%Y-%m', OrderDate).)",
                Ordered: true,
                "SELECT strftime('%Y-%m', o.OrderDate) AS Month,\n"
                + "       SUM(oi.Quantity * oi.UnitPrice) AS Revenue\n"
                + "FROM Orders o\n"
                + "JOIN OrderItems oi ON oi.OrderId = o.Id\n"
                + "WHERE o.Status = 'Completed'\n"
                + "GROUP BY Month\n"
                + "ORDER BY Month;",
                starter),

            // -------------------- Senior --------------------
            new("Senior.CustomerRevenueRanking", "Senior",
                "Customer Revenue Ranking",
                "Rank customers by their total 'Completed' revenue (highest = rank 1). Return the customer Name "
                + "and their Rank, ordered by rank. Only include customers with completed orders.",
                Ordered: true,
                "SELECT c.Name,\n"
                + "       ROW_NUMBER() OVER (ORDER BY SUM(oi.Quantity * oi.UnitPrice) DESC) AS Rank\n"
                + "FROM Customers c\n"
                + "JOIN Orders o ON o.CustomerId = c.Id AND o.Status = 'Completed'\n"
                + "JOIN OrderItems oi ON oi.OrderId = o.Id\n"
                + "GROUP BY c.Id, c.Name\n"
                + "ORDER BY Rank;",
                starter),

            new("Senior.SecondHighestProductPrice", "Senior",
                "Second Highest Product Price",
                "Return a single value: the second-highest distinct product price.",
                Ordered: false,
                "SELECT Price FROM (\n"
                + "    SELECT DISTINCT Price FROM Products ORDER BY Price DESC LIMIT 2\n"
                + ") ORDER BY Price ASC LIMIT 1;",
                starter),

            new("Senior.RunningMonthlyRevenue", "Senior",
                "Running Monthly Revenue",
                "For each calendar month, return the month ('YYYY-MM'), that month's 'Completed' revenue, and the "
                + "running cumulative revenue through that month, ordered by month.",
                Ordered: true,
                "SELECT Month, Revenue, SUM(Revenue) OVER (ORDER BY Month) AS RunningTotal\n"
                + "FROM (\n"
                + "    SELECT strftime('%Y-%m', o.OrderDate) AS Month,\n"
                + "           SUM(oi.Quantity * oi.UnitPrice) AS Revenue\n"
                + "    FROM Orders o\n"
                + "    JOIN OrderItems oi ON oi.OrderId = o.Id\n"
                + "    WHERE o.Status = 'Completed'\n"
                + "    GROUP BY Month\n"
                + ")\n"
                + "ORDER BY Month;",
                starter),

            new("Senior.OrdersAboveAverageValue", "Senior",
                "Orders Above Average Value",
                "Return the Id of every 'Completed' order whose total value is greater than the average total "
                + "value across all completed orders, ordered by order id. (Order total = SUM(Quantity * UnitPrice).)",
                Ordered: true,
                "WITH OrderTotals AS (\n"
                + "    SELECT o.Id AS OrderId, SUM(oi.Quantity * oi.UnitPrice) AS Total\n"
                + "    FROM Orders o\n"
                + "    JOIN OrderItems oi ON oi.OrderId = o.Id\n"
                + "    WHERE o.Status = 'Completed'\n"
                + "    GROUP BY o.Id\n"
                + ")\n"
                + "SELECT OrderId FROM OrderTotals\n"
                + "WHERE Total > (SELECT AVG(Total) FROM OrderTotals)\n"
                + "ORDER BY OrderId;",
                starter),
        };
    }
}
