using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace InterviewProblems.Infrastructure;

/// <summary>
/// The result of running a SQL query: the column names and the rows returned.
/// </summary>
public sealed record QueryResult(IReadOnlyList<string> Columns, IReadOnlyList<object?[]> Rows);

public static class SqlQueryPolicy
{
    private static readonly Regex DangerousSql = new(
        @"\b(ATTACH|DETACH|PRAGMA|CREATE|INSERT|UPDATE|DELETE|REPLACE|DROP|ALTER|VACUUM|REINDEX|ANALYZE)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static bool IsReadOnlySingleQuery(string sql)
    {
        var trimmed = sql.Trim();
        if (trimmed.EndsWith(';'))
        {
            trimmed = trimmed[..^1].TrimEnd();
        }

        if (trimmed.Contains(';') || DangerousSql.IsMatch(trimmed))
        {
            return false;
        }

        return Regex.IsMatch(
            trimmed,
            @"^(?:--[^\r\n]*(?:\r?\n|$)|/\*.*?\*/\s*)*(SELECT|WITH|EXPLAIN)\b",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
    }
}

/// <summary>
/// Builds a fresh, seeded in-memory SQLite database from the embedded
/// <c>schema.sql</c> and <c>seed.sql</c> scripts. Shared by the test suite and
/// the interactive console runner so both see identical data.
/// </summary>
public static class InterviewDatabase
{
    private static readonly string SchemaSql = ReadEmbedded("schema.sql");
    private static readonly string SeedSql = ReadEmbedded("seed.sql");

    /// <summary>
    /// Open a new in-memory SQLite connection and apply the schema and seed data.
    /// The caller owns the connection and must dispose it.
    /// </summary>
    public static SqliteConnection CreateSeededConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        ExecuteScript(connection, SchemaSql);
        ExecuteScript(connection, SeedSql);
        return connection;
    }

    /// <summary>
    /// Run <paramref name="sql"/> against a freshly seeded database and return
    /// the full result set. Convenience wrapper for one-shot queries.
    /// </summary>
    public static QueryResult Query(string sql)
    {
        using var connection = CreateSeededConnection();
        return Query(connection, sql);
    }

    /// <summary>Run <paramref name="sql"/> against an existing connection.</summary>
    public static QueryResult Query(SqliteConnection connection, string sql)
    {
        if (!SqlQueryPolicy.IsReadOnlySingleQuery(sql))
        {
            throw new InvalidOperationException(
                "Only one read-only SELECT, WITH, or EXPLAIN query is allowed.");
        }

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();

        var columns = new string[reader.FieldCount];
        for (var i = 0; i < reader.FieldCount; i++)
        {
            columns[i] = reader.GetName(i);
        }

        var rows = new List<object?[]>();
        while (reader.Read())
        {
            var values = new object?[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            rows.Add(values);
        }

        return new QueryResult(columns, rows);
    }

    private static void ExecuteScript(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static string ReadEmbedded(string fileName)
    {
        var assembly = typeof(InterviewDatabase).GetTypeInfo().Assembly;
        var resourceName = $"InterviewProblems.Data.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' was not found. " +
                "Ensure Data/*.sql are marked as EmbeddedResource in the project file.");

        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }
}
