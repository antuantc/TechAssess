namespace InterviewProblems.Developer2;

/// <summary>
/// Developer 2 — Collection and LINQ problems.
///
/// Implement each method so that the matching tests in
/// <c>tests/InterviewProblems.Tests/Coding/CollectionProblemsTests.cs</c> pass.
/// </summary>
public class CollectionProblems
{
    /// <summary>
    /// Return the values that appear more than once in the sequence.
    /// Each duplicated value is returned exactly once, in the order in which
    /// it was first detected as a duplicate (i.e. on its second occurrence).
    /// Example: [1, 2, 3, 2, 4, 1, 1] -> [2, 1].
    /// </summary>
    public IEnumerable<int> FindDuplicates(IEnumerable<int> numbers)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Group the words by their length.
    /// The key is the word length; the value is the list of words with that
    /// length, preserving their original order.
    /// Null or empty words are ignored.
    /// </summary>
    public IDictionary<int, List<string>> GroupByLength(IEnumerable<string> words)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the <paramref name="n"/> most frequently occurring items.
    /// Order the result by frequency (highest first). Break ties by first
    /// appearance in the input. If <paramref name="n"/> is greater than the
    /// number of distinct items, return all distinct items.
    /// Example: (["a","b","a","c","b","a"], 2) -> ["a", "b"].
    /// </summary>
    public IEnumerable<T> TopNFrequent<T>(IEnumerable<T> items, int n) where T : notnull
    {
        throw new NotImplementedException();
    }
}
