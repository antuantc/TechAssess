namespace InterviewProblems.Developer2;

/// <summary>
/// Developer 2 — Algorithm and recursion problems.
///
/// Implement each method so that the matching tests in
/// <c>tests/InterviewProblems.Tests/Coding/AlgorithmProblemsTests.cs</c> pass.
/// </summary>
public class AlgorithmProblems
{
    /// <summary>
    /// Given an array of integers and a target, return the indices of the two
    /// numbers that add up to the target. Assume at most one valid answer.
    /// Return the indices in ascending order. If no pair exists, return an
    /// empty array. Aim for a single-pass O(n) solution.
    /// Example: ([2, 7, 11, 15], 9) -> [0, 1].
    /// </summary>
    public int[] TwoSum(int[] numbers, int target)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the nth Fibonacci number (0-indexed: 0, 1, 1, 2, 3, 5, 8, ...).
    /// Must run efficiently for large n (iterative or memoized — no naive
    /// exponential recursion). Throw <see cref="ArgumentOutOfRangeException"/>
    /// for negative input.
    /// </summary>
    public long Fibonacci(int n)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Flatten an arbitrarily nested list of integers into a single sequence,
    /// preserving left-to-right order. Each element is either an
    /// <see cref="int"/> or another <see cref="IEnumerable{Object}"/>.
    /// Example: [1, [2, [3, 4]], 5] -> [1, 2, 3, 4, 5].
    /// </summary>
    public IEnumerable<int> Flatten(IEnumerable<object> nested)
    {
        throw new NotImplementedException();
    }
}
