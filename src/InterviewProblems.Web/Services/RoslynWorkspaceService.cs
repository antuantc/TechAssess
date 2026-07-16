using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Tags;
using Microsoft.CodeAnalysis.Text;

namespace InterviewProblems.Web.Services;

/// <summary>A single IntelliSense suggestion sent to Monaco.</summary>
public sealed record CompletionEntry(string Label, string InsertText, string Kind, string Detail);

/// <summary>A single editor marker (squiggle) sent to Monaco.</summary>
public sealed record EditorMarker(
    string Message, int Severity,
    int StartLineNumber, int StartColumn, int EndLineNumber, int EndColumn);

/// <summary>
/// Hosts a Roslyn <see cref="AdhocWorkspace"/> to power the editor experience:
/// semantic completions and live diagnostics for the class the candidate edits.
/// </summary>
public sealed class RoslynWorkspaceService
{
    // Mirrors the ImplicitUsings the real InterviewProblems project relies on,
    // so the candidate's stub (which has no explicit usings) resolves correctly.
    private static readonly string[] GlobalUsings =
    {
        "System",
        "System.Collections.Generic",
        "System.Collections.Concurrent",
        "System.IO",
        "System.Linq",
        "System.Net.Http",
        "System.Text",
        "System.Threading",
        "System.Threading.Tasks",
    };

    private readonly IReadOnlyList<MetadataReference> _references;
    private readonly CSharpCompilationOptions _options;

    // NOTE: CSharpCompilationOptions.Usings only affects *script* compilations,
    // so global usings must be supplied as a real 'global using' source unit.
    private static readonly string GlobalUsingsSource =
        string.Join("\n", GlobalUsings.Select(u => $"global using {u};"));

    private static readonly SyntaxTree GlobalUsingsTree =
        CSharpSyntaxTree.ParseText(GlobalUsingsSource, path: "GlobalUsings.cs");

    // AdhocWorkspace's default host only includes the Workspaces assemblies, so
    // CompletionService.GetService would return null. Add the Features assemblies
    // so semantic completions are available.
    private static readonly MefHostServices Host = MefHostServices.Create(
        MefHostServices.DefaultAssemblies
            .Concat(new[]
            {
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
            })
            .Distinct());

    public RoslynWorkspaceService(ReferenceProvider references)
    {
        _references = references.ForEditor();
        _options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
    }

    /// <summary>Semantic completions at <paramref name="offset"/> within <paramref name="code"/>.</summary>
    public async Task<IReadOnlyList<CompletionEntry>> GetCompletionsAsync(string code, int offset)
    {
        if (offset < 0 || offset > code.Length)
        {
            offset = Math.Clamp(offset, 0, code.Length);
        }

        using var workspace = new AdhocWorkspace(Host);
        var document = CreateDocument(workspace, code);

        var completionService = CompletionService.GetService(document);
        if (completionService is null)
        {
            return Array.Empty<CompletionEntry>();
        }

        var completions = await completionService.GetCompletionsAsync(document, offset);
        if (completions is null)
        {
            return Array.Empty<CompletionEntry>();
        }

        var entries = new List<CompletionEntry>(capacity: completions.ItemsList.Count);
        foreach (var item in completions.ItemsList.Take(200))
        {
            entries.Add(new CompletionEntry(
                Label: item.DisplayText,
                InsertText: string.IsNullOrEmpty(item.DisplayText) ? item.FilterText : item.DisplayText,
                Kind: MapKind(item.Tags),
                Detail: item.InlineDescription ?? string.Empty));
        }

        return entries;
    }

    /// <summary>Compiler diagnostics for the candidate's class, mapped to editor markers.</summary>
    public IReadOnlyList<EditorMarker> GetDiagnostics(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code, path: "Problem.cs");
        var compilation = CSharpCompilation.Create(
            "EditorDiagnostics",
            new[] { GlobalUsingsTree, tree },
            _references,
            _options);

        var markers = new List<EditorMarker>();
        foreach (var diagnostic in compilation.GetDiagnostics())
        {
            if (diagnostic.Severity is not (DiagnosticSeverity.Error or DiagnosticSeverity.Warning))
            {
                continue;
            }

            // Only surface problems in the candidate's own file.
            if (!diagnostic.Location.IsInSource || diagnostic.Location.SourceTree != tree)
            {
                continue;
            }

            var span = diagnostic.Location.GetLineSpan().Span;
            markers.Add(new EditorMarker(
                Message: diagnostic.GetMessage(),
                Severity: diagnostic.Severity == DiagnosticSeverity.Error ? 8 : 4, // Monaco: Error=8, Warning=4
                StartLineNumber: span.Start.Line + 1,
                StartColumn: span.Start.Character + 1,
                EndLineNumber: span.End.Line + 1,
                EndColumn: span.End.Character + 1));
        }

        return markers;
    }

    private Document CreateDocument(AdhocWorkspace workspace, string code)
    {
        var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                name: "Candidate",
                assemblyName: "Candidate",
                language: LanguageNames.CSharp)
            .WithMetadataReferences(_references)
            .WithCompilationOptions(_options);

        var project = workspace.AddProject(projectInfo);
        workspace.AddDocument(project.Id, "GlobalUsings.cs", SourceText.From(GlobalUsingsSource));
        return workspace.AddDocument(project.Id, "Problem.cs", SourceText.From(code));
    }

    private static string MapKind(IEnumerable<string> tags)
    {
        foreach (var tag in tags)
        {
            switch (tag)
            {
                case WellKnownTags.Method: return "Method";
                case WellKnownTags.Property: return "Property";
                case WellKnownTags.Field: return "Field";
                case WellKnownTags.Local: return "Variable";
                case WellKnownTags.Parameter: return "Variable";
                case WellKnownTags.Class: return "Class";
                case WellKnownTags.Interface: return "Interface";
                case WellKnownTags.Enum: return "Enum";
                case WellKnownTags.Structure: return "Struct";
                case WellKnownTags.Delegate: return "Function";
                case WellKnownTags.Namespace: return "Module";
                case WellKnownTags.Keyword: return "Keyword";
                case WellKnownTags.EnumMember: return "EnumMember";
                case WellKnownTags.Event: return "Event";
                case WellKnownTags.Constant: return "Constant";
            }
        }

        return "Text";
    }
}
