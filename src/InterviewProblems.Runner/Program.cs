using InterviewProblems.Infrastructure;
using Microsoft.Data.Sqlite;

// Interactive SQL explorer for the interview. Opens a fresh, seeded in-memory
// SQLite database (identical to the one the tests use) so candidates and
// interviewers can experiment with queries and see results as a table.

using var connection = InterviewDatabase.CreateSeededConnection();

Console.WriteLine("=== Interview SQL Explorer ===");
Console.WriteLine("In-memory SQLite, seeded with the interview data set.");
Console.WriteLine("Commands:  \\schema   \\tables   \\help   \\q (quit)");
Console.WriteLine("Type a SQL statement and press Enter to run it.");
PrintSchema();

while (true)
{
    Console.Write("\nsql> ");
    var line = Console.ReadLine();
    if (line is null)
    {
        break; // EOF (e.g. piped input finished)
    }

    line = line.Trim();
    if (line.Length == 0)
    {
        continue;
    }

    switch (line.ToLowerInvariant())
    {
        case "\\q":
        case "\\quit":
        case "exit":
        case "quit":
            return;
        case "\\help":
        case "help":
            Console.WriteLine("Commands:  \\schema   \\tables   \\help   \\q (quit)");
            continue;
        case "\\schema":
            PrintSchema();
            continue;
        case "\\tables":
            RunAndPrint("SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;");
            continue;
        default:
            RunAndPrint(line);
            continue;
    }
}

void RunAndPrint(string sql)
{
    try
    {
        var result = InterviewDatabase.Query(connection, sql);
        if (result.Columns.Count == 0)
        {
            Console.WriteLine("(statement executed; no result set)");
            return;
        }

        PrintTable(result);
        Console.WriteLine($"({result.Rows.Count} row{(result.Rows.Count == 1 ? "" : "s")})");
    }
    catch (SqliteException ex)
    {
        Console.WriteLine($"SQL error: {ex.Message}");
    }
}

void PrintTable(QueryResult result)
{
    var widths = new int[result.Columns.Count];
    for (var i = 0; i < result.Columns.Count; i++)
    {
        widths[i] = result.Columns[i].Length;
    }

    var cells = result.Rows
        .Select(row => row.Select(Format).ToArray())
        .ToList();

    foreach (var row in cells)
    {
        for (var i = 0; i < row.Length; i++)
        {
            widths[i] = Math.Max(widths[i], row[i].Length);
        }
    }

    var header = string.Join(" | ", result.Columns.Select((c, i) => c.PadRight(widths[i])));
    var divider = string.Join("-+-", widths.Select(w => new string('-', w)));
    Console.WriteLine(header);
    Console.WriteLine(divider);

    foreach (var row in cells)
    {
        Console.WriteLine(string.Join(" | ", row.Select((c, i) => c.PadRight(widths[i]))));
    }
}

static string Format(object? value) => value switch
{
    null => "NULL",
    double d => d.ToString("0.##"),
    _ => value.ToString() ?? string.Empty
};

void PrintSchema()
{
    Console.WriteLine();
    Console.WriteLine("Tables:");
    Console.WriteLine("  Customers (Id, Name, Email, City, IsActive, CreatedAt)");
    Console.WriteLine("  Products  (Id, Name, Category, Price)");
    Console.WriteLine("  Orders    (Id, CustomerId, OrderDate, Status)");
    Console.WriteLine("  OrderItems(Id, OrderId, ProductId, Quantity, UnitPrice)");
}
