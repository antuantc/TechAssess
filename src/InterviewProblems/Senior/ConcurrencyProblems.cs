namespace InterviewProblems.Senior;

/// <summary>
/// Senior — Concurrency and parallelism.
///
/// Implement each method so that the matching tests in
/// <c>tests/InterviewProblems.Tests/Senior/ConcurrencyProblemsTests.cs</c> pass.
/// These exercise bounded concurrency and thread-safe aggregation.
/// </summary>
public class ConcurrencyProblems
{
    /// <summary>
    /// Apply <paramref name="operation"/> to every item, running at most
    /// <paramref name="maxConcurrency"/> operations at the same time.
    /// The returned list must be in the same order as <paramref name="source"/>,
    /// regardless of the order in which the operations complete.
    /// Throw <see cref="ArgumentOutOfRangeException"/> if maxConcurrency &lt; 1.
    /// (Hint: <see cref="System.Threading.SemaphoreSlim"/>.)
    /// </summary>
    public Task<IReadOnlyList<TResult>> MapWithConcurrencyLimitAsync<TSource, TResult>(
        IReadOnlyList<TSource> source,
        int maxConcurrency,
        Func<TSource, Task<TResult>> operation)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sum the numbers using multiple threads. The result must be correct under
    /// concurrent access (no lost updates).
    /// (Hint: <see cref="System.Threading.Interlocked"/> or a lock.)
    /// </summary>
    public long SumConcurrently(IEnumerable<long> numbers)
    {
        throw new NotImplementedException();
    }
}
