namespace InterviewProblems.Web.Services;

/// <summary>
/// One editable problem: the class the candidate implements plus the test
/// file(s) that grade it and any support source needed to compile those tests.
/// </summary>
public sealed class ProblemSet
{
    public required string Id { get; init; }
    public required string Level { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required string Category { get; init; }
    public required bool IsSql { get; init; }

    /// <summary>The source file the candidate edits (its current on-disk content is the stub).</summary>
    public required string ClassFilePath { get; init; }

    /// <summary>Test source files that grade the candidate's implementation.</summary>
    public required IReadOnlyList<string> TestFilePaths { get; init; }

    /// <summary>Extra source (Infrastructure, shared helpers) compiled alongside the tests.</summary>
    public required IReadOnlyList<string> SupportFilePaths { get; init; }

    /// <summary>The stub source read from disk — the candidate's starting point.</summary>
    public string StubCode => File.ReadAllText(ClassFilePath);
}

/// <summary>
/// Discovers the interview problems by scanning the real
/// <c>src/InterviewProblems/&lt;Level&gt;</c> folders and matching each problem
/// class to its <c>&lt;Name&gt;Tests.cs</c> file under the test project.
/// </summary>
public sealed class ProblemCatalog
{
    private static readonly string[] Levels = { "Developer1", "Developer2", "Senior" };

    private readonly RepoLocator _repo;
    private readonly List<ProblemSet> _sets;

    public ProblemCatalog(RepoLocator repo)
    {
        _repo = repo;
        _sets = Build();
    }

    public IReadOnlyList<ProblemSet> All => _sets;

    public ProblemSet? Find(string id) => _sets.FirstOrDefault(s => s.Id == id);

    public IEnumerable<IGrouping<string, ProblemSet>> ByLevel =>
        _sets.GroupBy(s => s.Level);

    private List<ProblemSet> Build()
    {
        var sets = new List<ProblemSet>();

        foreach (var level in Levels)
        {
            var srcDir = Path.Combine(_repo.ProblemsProjectDir, level);
            var testDir = Path.Combine(_repo.TestsProjectDir, level);
            if (!Directory.Exists(srcDir))
            {
                continue;
            }

            foreach (var classFile in Directory.GetFiles(srcDir, "*.cs").OrderBy(f => f))
            {
                var name = Path.GetFileNameWithoutExtension(classFile);
                var testFile = Path.Combine(testDir, $"{name}Tests.cs");
                if (!File.Exists(testFile))
                {
                    // No grading tests for this file — skip it.
                    continue;
                }

                var isSql = name.Contains("Sql", StringComparison.OrdinalIgnoreCase);
                var support = new List<string>();
                if (isSql)
                {
                    support.Add(_repo.InfrastructureSourcePath);
                    support.Add(_repo.SqlTestHelperSourcePath);
                }

                sets.Add(new ProblemSet
                {
                    Id = $"{level}.{name}",
                    Level = level,
                    Name = name,
                    DisplayName = Humanize(name),
                    Category = isSql ? "SQL" : Humanize(name.Replace("Problems", string.Empty)),
                    IsSql = isSql,
                    ClassFilePath = classFile,
                    TestFilePaths = new[] { testFile },
                    SupportFilePaths = support,
                });
            }
        }

        return sets;
    }

    private static string Humanize(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return identifier;
        }

        var spaced = System.Text.RegularExpressions.Regex.Replace(
            identifier, "(?<=[a-z0-9])(?=[A-Z])", " ");
        return spaced;
    }
}
