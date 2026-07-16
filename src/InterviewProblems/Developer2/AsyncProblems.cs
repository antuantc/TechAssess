namespace InterviewProblems.Developer2;

/// <summary>
/// Developer 2 — Asynchronous programming with Task.
///
/// Implement each method so that the matching tests in
/// <c>tests/InterviewProblems.Tests/Developer2/AsyncProblemsTests.cs</c> pass.
/// These exercise <c>async</c>/<c>await</c>, <see cref="Task.WhenAll(System.Threading.Tasks.Task[])"/>,
/// <see cref="Task.WhenAny(System.Threading.Tasks.Task[])"/>, and error handling.
/// </summary>
public class AsyncProblems
{
    /// <summary>
    /// Await every task and return the sum of all the results.
    /// The tasks may complete in any order; run them concurrently.
    /// </summary>
    public Task<long> SumAsync(IEnumerable<Task<long>> tasks)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the result of whichever task completes first.
    /// </summary>
    public Task<T> FirstToCompleteAsync<T>(IEnumerable<Task<T>> tasks)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Invoke <paramref name="action"/> and return its result. If it throws,
    /// retry up to <paramref name="maxAttempts"/> total attempts. If every
    /// attempt fails, rethrow the exception from the final attempt.
    /// Throw <see cref="ArgumentOutOfRangeException"/> if maxAttempts &lt; 1.
    /// </summary>
    public Task<T> WithRetryAsync<T>(Func<Task<T>> action, int maxAttempts)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the task's result if it completes within <paramref name="timeout"/>;
    /// otherwise throw <see cref="TimeoutException"/>.
    /// </summary>
    public Task<T> WithTimeoutAsync<T>(Task<T> task, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }
}
