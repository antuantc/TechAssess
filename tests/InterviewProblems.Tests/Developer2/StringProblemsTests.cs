using InterviewProblems.Developer2;

namespace InterviewProblems.Tests.Developer2;

[Trait("Level", "Developer2")]
[Trait("Category", "Strings")]
public class StringProblemsTests
{
    private readonly StringProblems _sut = new();

    [Theory]
    [InlineData("the quick brown fox", "fox brown quick the")]
    [InlineData("hello", "hello")]
    [InlineData("  lots   of   space  ", "space of lots")]
    [InlineData("", "")]
    public void ReverseWords_reverses_word_order(string input, string expected)
    {
        Assert.Equal(expected, _sut.ReverseWords(input));
    }

    [Theory]
    [InlineData("listen", "silent", true)]
    [InlineData("Dormitory", "Dirty room", true)]
    [InlineData("hello", "world", false)]
    [InlineData("Astronomer", "Moon starer", true)]
    public void AreAnagrams_detects_anagrams(string first, string second, bool expected)
    {
        Assert.Equal(expected, _sut.AreAnagrams(first, second));
    }

    [Theory]
    [InlineData("swiss", 'w')]
    [InlineData("aabbcc", null)]
    [InlineData("stress", 't')]
    [InlineData("", null)]
    public void FirstUniqueCharacter_returns_first_non_repeating(string input, char? expected)
    {
        Assert.Equal(expected, _sut.FirstUniqueCharacter(input));
    }
}
