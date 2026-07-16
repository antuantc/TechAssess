using Microsoft.CodeAnalysis;

namespace InterviewProblems.Web.Services;

/// <summary>
/// Builds the metadata reference set from the running app's trusted platform
/// assemblies, cached once. Provides variants for the editor (IntelliSense) and
/// for the candidate compile/run harness.
/// </summary>
public sealed class ReferenceProvider
{
    private readonly Lazy<IReadOnlyList<PortableExecutableReference>> _platform;

    public ReferenceProvider()
    {
        _platform = new Lazy<IReadOnlyList<PortableExecutableReference>>(LoadPlatformReferences);
    }

    /// <summary>
    /// All trusted platform assemblies plus the InterviewProblems assembly, for
    /// the editor's IntelliSense (so Infrastructure types resolve).
    /// </summary>
    public IReadOnlyList<MetadataReference> ForEditor()
    {
        var refs = new List<MetadataReference>(_platform.Value);
        AddIfPresent(refs, typeof(Infrastructure.InterviewDatabase).Assembly.Location);
        return refs;
    }

    /// <summary>
    /// Trusted platform assemblies EXCEPT InterviewProblems.dll. The candidate's
    /// problem class is recompiled from source, so referencing the original
    /// assembly would create a duplicate-type ambiguity in the test code.
    /// xUnit and SQLite are already in the platform set (this app references
    /// them), so the compiled tests bind to the exact same types this host runs.
    /// </summary>
    public IReadOnlyList<MetadataReference> ForCandidate()
    {
        return _platform.Value
            .Where(r => !string.Equals(
                Path.GetFileName(r.FilePath), "InterviewProblems.dll",
                StringComparison.OrdinalIgnoreCase))
            .ToList<MetadataReference>();
    }

    private static IReadOnlyList<PortableExecutableReference> LoadPlatformReferences()
    {
        var tpa = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string) ?? string.Empty;
        var refs = new List<PortableExecutableReference>();
        foreach (var path in tpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                refs.Add(MetadataReference.CreateFromFile(path));
            }
            catch
            {
                // Skip anything that can't be read as a managed assembly.
            }
        }

        return refs;
    }

    private static void AddIfPresent(List<MetadataReference> refs, string location)
    {
        if (!string.IsNullOrEmpty(location) &&
            !refs.OfType<PortableExecutableReference>().Any(r =>
                string.Equals(r.FilePath, location, StringComparison.OrdinalIgnoreCase)))
        {
            refs.Add(MetadataReference.CreateFromFile(location));
        }
    }
}
