using InterviewProblems.Developer2;

namespace InterviewProblems.Tests.Developer2;

[Trait("Level", "Developer2")]
[Trait("Category", "Algorithms")]
public class AlgorithmProblemsTests
{
    private readonly AlgorithmProblems _sut = new();

    [Theory]
    [InlineData(new[] { 2, 7, 11, 15 }, 9, new[] { 0, 1 })]
    [InlineData(new[] { 3, 2, 4 }, 6, new[] { 1, 2 })]
    [InlineData(new[] { 1, 2, 3 }, 100, new int[0])]
    public void TwoSum_finds_index_pair(int[] numbers, int target, int[] expected)
    {
        Assert.Equal(expected, _sut.TwoSum(numbers, target));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(10, 55)]
    [InlineData(50, 12586269025)]
    public void Fibonacci_returns_expected_value(int n, long expected)
    {
        Assert.Equal(expected, _sut.Fibonacci(n));
    }

    [Fact]
    public void Fibonacci_throws_for_negative_input()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Fibonacci(-1));
    }

    [Fact]
    public void Flatten_flattens_nested_lists()
    {
        var nested = new List<object>
        {
            1,
            new List<object> { 2, new List<object> { 3, 4 } },
            5
        };

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, _sut.Flatten(nested));
    }

    [Fact]
    public void Flatten_handles_flat_list()
    {
        var flat = new List<object> { 1, 2, 3 };
        Assert.Equal(new[] { 1, 2, 3 }, _sut.Flatten(flat));
    }
}
