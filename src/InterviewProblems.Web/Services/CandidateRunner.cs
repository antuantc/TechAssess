using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Sdk;

namespace InterviewProblems.Web.Services;

public sealed record CompileError(string Message, int Line, int Column, bool InCandidateCode);

public sealed record TestOutcome(string Name, bool Passed, string? FailureMessage);

public sealed class RunResult
{
    public bool Compiled { get; init; }
    public IReadOnlyList<CompileError> Errors { get; init; } = Array.Empty<CompileError>();
    public IReadOnlyList<TestOutcome> Tests { get; init; } = Array.Empty<TestOutcome>();

    public int Passed => Tests.Count(t => t.Passed);
    public int Total => Tests.Count;
    public bool AllPassed => Compiled && Total > 0 && Passed == Total;
}

/// <summary>
/// Compiles the candidate's edited class together with the real interview tests
/// into a throwaway assembly, then runs those tests with a lightweight reflection
/// runner over the genuine xUnit attributes and asserts.
/// </summary>
public sealed class CandidateRunner
{
    // The candidate + test sources rely on ImplicitUsings and the test project's
    // global "using Xunit;" — supply both here so everything compiles.
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
        "Xunit",
    };

    // CSharpCompilationOptions.Usings is ignored for non-script compilations, so
    // the ImplicitUsings/global "using Xunit;" are supplied as a real source unit.
    private static readonly string GlobalUsingsSource =
        string.Join("\n", GlobalUsings.Select(u => $"global using {u};"));

    private readonly ReferenceProvider _references;
    private readonly RepoLocator _repo;

    public CandidateRunner(ReferenceProvider references, RepoLocator repo)
    {
        _references = references;
        _repo = repo;
    }

    public async Task<RunResult> RunAsync(ProblemSet set, string candidateCode, CancellationToken ct = default)
    {
        var candidateTree = CSharpSyntaxTree.ParseText(candidateCode, path: "Candidate.cs");
        var trees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(GlobalUsingsSource, path: "GlobalUsings.cs"),
            candidateTree,
        };

        foreach (var support in set.SupportFilePaths)
        {
            trees.Add(CSharpSyntaxTree.ParseText(await File.ReadAllTextAsync(support, ct), path: support));
        }

        foreach (var testFile in set.TestFilePaths)
        {
            trees.Add(CSharpSyntaxTree.ParseText(await File.ReadAllTextAsync(testFile, ct), path: testFile));
        }

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Debug);

        var compilation = CSharpCompilation.Create(
            $"Candidate_{Guid.NewGuid():N}",
            trees,
            _references.ForCandidate(),
            options);

        var resources = BuildSqlResources(set);

        using var peStream = new MemoryStream();
        var emit = compilation.Emit(peStream, manifestResources: resources);

        if (!emit.Success)
        {
            return new RunResult
            {
                Compiled = false,
                Errors = emit.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d =>
                    {
                        var span = d.Location.GetLineSpan();
                        var inCandidate = span.Path == "Candidate.cs";
                        return new CompileError(
                            d.GetMessage(),
                            span.Span.Start.Line + 1,
                            span.Span.Start.Character + 1,
                            inCandidate);
                    })
                    .ToList(),
            };
        }

        peStream.Seek(0, SeekOrigin.Begin);

        var context = new CollectibleContext();
        try
        {
            var assembly = context.LoadFromStream(peStream);
            var outcomes = await RunTestsAsync(assembly, ct);
            return new RunResult { Compiled = true, Tests = outcomes };
        }
        finally
        {
            context.Unload();
        }
    }

    private List<ResourceDescription> BuildSqlResources(ProblemSet set)
    {
        var resources = new List<ResourceDescription>();
        if (!set.IsSql)
        {
            return resources;
        }

        // InterviewDatabase reads these as embedded resources named
        // "InterviewProblems.Data.<file>" — recreate them for the candidate assembly.
        resources.Add(new ResourceDescription(
            "InterviewProblems.Data.schema.sql",
            () => File.OpenRead(_repo.SchemaSqlPath),
            isPublic: true));
        resources.Add(new ResourceDescription(
            "InterviewProblems.Data.seed.sql",
            () => File.OpenRead(_repo.SeedSqlPath),
            isPublic: true));
        return resources;
    }

    private static async Task<List<TestOutcome>> RunTestsAsync(Assembly assembly, CancellationToken ct)
    {
        var outcomes = new List<TestOutcome>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || type.GetConstructor(Type.EmptyTypes) is null)
            {
                continue;
            }

            var testMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<FactAttribute>() is not null
                            || m.GetCustomAttribute<TheoryAttribute>() is not null)
                .ToList();

            if (testMethods.Count == 0)
            {
                continue;
            }

            foreach (var method in testMethods)
            {
                ct.ThrowIfCancellationRequested();

                foreach (var (displayName, args) in ExpandCases(type, method))
                {
                    outcomes.Add(await RunOneAsync(type, method, displayName, args));
                }
            }
        }

        return outcomes.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
    }

    private static IEnumerable<(string DisplayName, object?[] Args)> ExpandCases(Type type, MethodInfo method)
    {
        var isTheory = method.GetCustomAttribute<TheoryAttribute>() is not null;
        if (!isTheory)
        {
            yield return ($"{type.Name}.{method.Name}", Array.Empty<object?>());
            yield break;
        }

        var dataAttributes = method.GetCustomAttributes<DataAttribute>().ToList();
        var index = 0;
        foreach (var data in dataAttributes)
        {
            foreach (var row in data.GetData(method))
            {
                var args = row ?? Array.Empty<object?>();
                yield return ($"{type.Name}.{method.Name}({FormatArgs(args)})", args);
                index++;
            }
        }

        if (index == 0)
        {
            yield return ($"{type.Name}.{method.Name}", Array.Empty<object?>());
        }
    }

    private static async Task<TestOutcome> RunOneAsync(Type type, MethodInfo method, string displayName, object?[] args)
    {
        object? instance;
        try
        {
            instance = Activator.CreateInstance(type);
        }
        catch (Exception ex)
        {
            return new TestOutcome(displayName, false, $"Could not construct test class: {Unwrap(ex).Message}");
        }

        try
        {
            var result = method.Invoke(instance, args);
            if (result is Task task)
            {
                await task.ConfigureAwait(false);
            }

            return new TestOutcome(displayName, true, null);
        }
        catch (Exception ex)
        {
            var inner = Unwrap(ex);
            var message = inner is XunitException
                ? inner.Message
                : $"{inner.GetType().Name}: {inner.Message}";
            return new TestOutcome(displayName, false, message);
        }
        finally
        {
            (instance as IDisposable)?.Dispose();
        }
    }

    private static Exception Unwrap(Exception ex)
    {
        while (ex is TargetInvocationException or AggregateException && ex.InnerException is not null)
        {
            ex = ex.InnerException;
        }

        return ex;
    }

    private static string FormatArgs(object?[] args) =>
        string.Join(", ", args.Select(FormatArg));

    private static string FormatArg(object? arg) => arg switch
    {
        null => "null",
        string s => $"\"{s}\"",
        System.Collections.IEnumerable e and not string =>
            "[" + string.Join(",", e.Cast<object?>().Select(FormatArg)) + "]",
        _ => arg.ToString() ?? string.Empty,
    };

    /// <summary>
    /// Collectible load context for the throwaway candidate assembly. Returning
    /// null from <see cref="Load"/> lets every dependency (framework, xUnit,
    /// SQLite) resolve from the default context, so their type identities match
    /// this host — essential for the reflection runner to see the real attributes.
    /// </summary>
    private sealed class CollectibleContext : AssemblyLoadContext
    {
        public CollectibleContext() : base(isCollectible: true) { }

        protected override Assembly? Load(AssemblyName assemblyName) => null;
    }
}
