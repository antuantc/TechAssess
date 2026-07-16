using InterviewProblems.Infrastructure;

namespace InterviewProblems.Tests;

/// <summary>
/// Thin test-facing wrapper over <see cref="InterviewDatabase"/>. Runs a
/// candidate's query against a freshly seeded in-memory database and exposes
/// the result in the shapes the SQL tests assert against.
/// </summary>
internal static class SqlTestDatabase
{
    /// <summary>Execute the query and return the single scalar value (first column of first row).</summary>
    public static object? Scalar(string query)
    {
        var result = InterviewDatabase.Query(query);
        return result.Rows.Count > 0 && result.Rows[0].Length > 0 ? result.Rows[0][0] : null;
    }

    /// <summary>Execute the query and return every row as an array of column values.</summary>
    public static IReadOnlyList<object?[]> Rows(string query)
        => InterviewDatabase.Query(query).Rows;

    /// <summary>Execute the query and return the first column of every row.</summary>
    public static IReadOnlyList<object?> Column(string query)
        => Rows(query).Select(row => row.Length > 0 ? row[0] : null).ToList();
}
