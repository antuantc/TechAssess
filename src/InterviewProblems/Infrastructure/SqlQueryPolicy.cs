namespace InterviewProblems.Infrastructure;

/// <summary>Validates that a SQL string contains one read-only query.</summary>
public static class SqlQueryPolicy
{
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

    public static bool IsReadOnlySingleQuery(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return false;
        }

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
                    return false;
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
                    return false;
                }

                continue;
            }

            if (sql[index] == ';')
            {
                if (statementEnded)
                {
                    return false;
                }

                statementEnded = true;
                index++;
                continue;
            }

            if (statementEnded)
            {
                return false;
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
                    return false;
                }

                continue;
            }

            index++;
        }

        return firstKeyword.Equals("SELECT", StringComparison.OrdinalIgnoreCase)
            || firstKeyword.Equals("WITH", StringComparison.OrdinalIgnoreCase)
            || firstKeyword.Equals("EXPLAIN", StringComparison.OrdinalIgnoreCase);
    }
}
