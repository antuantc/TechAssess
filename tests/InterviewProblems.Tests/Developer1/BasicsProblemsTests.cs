using InterviewProblems.Developer1;

namespace InterviewProblems.Tests.Developer1;

[Trait("Level", "Developer1")]
[Trait("Category", "Basics")]
public class BasicsProblemsTests
{
    private readonly BasicsProblems _sut = new();

    [Theory]
    [InlineData(new[] { 1, 2, 3, 4 }, 10)]
    [InlineData(new int[0], 0)]
    [InlineData(new[] { -5, 5 }, 0)]
    public void Sum_adds_all_numbers(int[] numbers, int expected)
    {
        Assert.Equal(expected, _sut.Sum(numbers));
    }

    [Fact]
    public void Max_returns_largest()
    {
        Assert.Equal(9, _sut.Max(new[] { 3, 9, 1, 7 }));
    }

    [Fact]
    public void Max_throws_on_empty()
    {
        Assert.Throws<ArgumentException>(() => _sut.Max(Array.Empty<int>()));
    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(3, "Fizz")]
    [InlineData(5, "Buzz")]
    [InlineData(15, "FizzBuzz")]
    [InlineData(30, "FizzBuzz")]
    public void FizzBuzz_maps_number(int number, string expected)
    {
        Assert.Equal(expected, _sut.FizzBuzz(number));
    }

    [Theory]
    [InlineData("hello", 2)]
    [InlineData("AEIOU", 5)]
    [InlineData("xyz", 0)]
    [InlineData("", 0)]
    public void CountVowels_counts_vowels(string input, int expected)
    {
        Assert.Equal(expected, _sut.CountVowels(input));
    }

    [Theory]
    [InlineData("hello", "olleh")]
    [InlineData("a", "a")]
    [InlineData("", "")]
    public void ReverseString_reverses(string input, string expected)
    {
        Assert.Equal(expected, _sut.ReverseString(input));
    }

    [Theory]
    [InlineData("Race car", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("A man a plan a canal Panama", true)]
    public void IsPalindrome_detects_palindromes(string input, bool expected)
    {
        Assert.Equal(expected, _sut.IsPalindrome(input));
    }
}
