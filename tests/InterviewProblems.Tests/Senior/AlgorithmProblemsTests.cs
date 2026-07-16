using InterviewProblems.Senior;

namespace InterviewProblems.Tests.Senior;

[Trait("Level", "Senior")]
[Trait("Category", "Algorithms")]
public class AlgorithmProblemsTests
{
    private readonly AlgorithmProblems _sut = new();

    [Fact]
    public void GroupAnagrams_groups_by_first_appearance()
    {
        var input = new[] { "eat", "tea", "tan", "ate", "nat", "bat" };

        var result = _sut.GroupAnagrams(input)
            .Select(group => group.ToArray())
            .ToArray();

        var expected = new[]
        {
            new[] { "eat", "tea", "ate" },
            new[] { "tan", "nat" },
            new[] { "bat" }
        };

        Assert.Equal(expected, result);
    }

    [Fact]
    public void MergeIntervals_merges_overlapping()
    {
        var input = new[]
        {
            new[] { 1, 3 },
            new[] { 2, 6 },
            new[] { 8, 10 },
            new[] { 15, 18 }
        };

        var result = _sut.MergeIntervals(input);

        var expected = new[]
        {
            new[] { 1, 6 },
            new[] { 8, 10 },
            new[] { 15, 18 }
        };

        Assert.Equal(expected, result);
    }

    [Fact]
    public void MergeIntervals_handles_unsorted_and_touching_intervals()
    {
        var input = new[]
        {
            new[] { 8, 10 },
            new[] { 1, 4 },
            new[] { 4, 5 }
        };

        var result = _sut.MergeIntervals(input);

        var expected = new[]
        {
            new[] { 1, 5 },
            new[] { 8, 10 }
        };

        Assert.Equal(expected, result);
    }
}
