using System.Text;
using System.Text.RegularExpressions;
using Markdig;

namespace InterviewProblems.Web.Services;

/// <summary>
/// A rendered set of verbal / conceptual questions for one level.
/// </summary>
public sealed class VerbalDoc
{
    public required string Level { get; init; }
    public required string DisplayLevel { get; init; }

    /// <summary>Full document, including the <c>*Looking for:*</c> interviewer hints.</summary>
    public required string InterviewerHtml { get; init; }

    /// <summary>Prompts only — the interviewer hint/follow-up lines are removed.</summary>
    public required string CandidateHtml { get; init; }
}

/// <summary>
/// Reads the verbal / conceptual questions from <c>docs/&lt;Level&gt;/Questions.md</c>
/// and renders them to HTML. Produces two variants: the full document for the
/// interviewer, and a candidate-safe version with the <c>*Looking for:*</c> and
/// <c>*Follow-up:*</c> answer hints stripped out. Read live from disk so edits to
/// the markdown are reflected without a restart.
/// </summary>
public sealed class VerbalQuestionProvider
{
    private static readonly string[] Levels = { "Developer1", "Developer2", "Senior" };

    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build();

    // A hint bullet: "- *Looking for...:*" or "- *Follow-up:*".
    private static readonly Regex HintStart =
        new(@"^\s*-\s*\*(Looking for|Follow-up)", RegexOptions.Compiled);

    private readonly RepoLocator _repo;

    public VerbalQuestionProvider(RepoLocator repo)
    {
        _repo = repo;
    }

    /// <summary>The levels that actually have a Questions.md on disk.</summary>
    public IReadOnlyList<(string Level, string DisplayLevel)> AvailableLevels() =>
        Levels
            .Where(l => File.Exists(_repo.QuestionsPath(l)))
            .Select(l => (l, DisplayLevel(l)))
            .ToList();

    /// <summary>Renders the questions for a level, or null if it has no document.</summary>
    public VerbalDoc? Get(string level)
    {
        var normalized = Levels.FirstOrDefault(
            l => string.Equals(l, level, StringComparison.OrdinalIgnoreCase));
        if (normalized is null)
        {
            return null;
        }

        var path = _repo.QuestionsPath(normalized);
        if (!File.Exists(path))
        {
            return null;
        }

        var markdown = File.ReadAllText(path);
        return new VerbalDoc
        {
            Level = normalized,
            DisplayLevel = DisplayLevel(normalized),
            InterviewerHtml = Markdown.ToHtml(markdown, Pipeline),
            CandidateHtml = Markdown.ToHtml(StripHints(markdown), Pipeline),
        };
    }

    /// <summary>
    /// Removes the interviewer-only hint blocks (<c>- *Looking for:*</c> and
    /// <c>- *Follow-up:*</c> bullets plus their wrapped/nested continuation lines)
    /// so a candidate sees the prompts and code but not the expected answers.
    /// </summary>
    private static string StripHints(string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder(markdown.Length);
        var inHint = false;

        foreach (var line in lines)
        {
            if (HintStart.IsMatch(line))
            {
                inHint = true;
                continue;
            }

            if (inHint)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Blank line ends the hint block; keep it to preserve spacing.
                    inHint = false;
                    sb.Append('\n');
                    continue;
                }

                if (char.IsWhiteSpace(line[0]))
                {
                    // Indented continuation or nested bullet of the hint — drop it.
                    continue;
                }

                // Non-indented content starts a new block — keep it.
                inHint = false;
            }

            sb.Append(line).Append('\n');
        }

        return sb.ToString();
    }

    private static string DisplayLevel(string level) => level switch
    {
        "Developer1" => "Developer 1",
        "Developer2" => "Developer 2",
        _ => level,
    };
}
