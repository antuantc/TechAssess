using Microsoft.AspNetCore.Hosting;

namespace InterviewProblems.Web.Services;

/// <summary>
/// Locates the repository root (the folder containing
/// <c>TechAssess.slnx</c>) by walking up from the app's content root.
/// Every other service reads the real problem and test source from disk relative
/// to this, so the web app always reflects the actual interview files.
/// </summary>
public sealed class RepoLocator
{
    public string RepoRoot { get; }

    public RepoLocator(IWebHostEnvironment env)
    {
        var dir = new DirectoryInfo(env.ContentRootPath);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "TechAssess.slnx")))
        {
            dir = dir.Parent;
        }

        RepoRoot = dir?.FullName
            ?? throw new InvalidOperationException(
                "Could not locate TechAssess.slnx above the content root. " +
                "Run the web app from within the repository.");
    }

    public string ProblemsProjectDir => Path.Combine(RepoRoot, "src", "InterviewProblems");

    public string TestsProjectDir => Path.Combine(RepoRoot, "tests", "InterviewProblems.Tests");

    public string DocsDir => Path.Combine(RepoRoot, "docs");

    /// <summary>The verbal / conceptual questions markdown for a given level.</summary>
    public string QuestionsPath(string level) => Path.Combine(DocsDir, level, "Questions.md");

    public string SchemaSqlPath => Path.Combine(ProblemsProjectDir, "Data", "schema.sql");

    public string SeedSqlPath => Path.Combine(ProblemsProjectDir, "Data", "seed.sql");

    public string InfrastructureSourcePath =>
        Path.Combine(ProblemsProjectDir, "Infrastructure", "InterviewDatabase.cs");

    public string SqlTestHelperSourcePath =>
        Path.Combine(TestsProjectDir, "Common", "SqlTestDatabase.cs");
}
