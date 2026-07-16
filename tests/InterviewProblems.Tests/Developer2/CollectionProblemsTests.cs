using InterviewProblems.Developer2;

namespace InterviewProblems.Tests.Developer2;

[Trait("Level", "Developer2")]
[Trait("Category", "Collections")]
public class CollectionProblemsTests
{
    private readonly CollectionProblems _sut = new();

    [Fact]
    public void FindDuplicates_returns_duplicates_in_detection_order()
    {
        var result = _sut.FindDuplicates(new[] { 1, 2, 3, 2, 4, 1, 1 });
        Assert.Equal(new[] { 2, 1 }, result);
    }

    [Fact]
    public void FindDuplicates_returns_empty_when_all_unique()
    {
        var result = _sut.FindDuplicates(new[] { 1, 2, 3, 4 });
        Assert.Empty(result);
    }

    [Fact]
    public void GroupByLength_groups_words_by_length()
    {
        var result = _sut.GroupByLength(new[] { "a", "bb", "cc", "ddd", "e" });

        Assert.Equal(new[] { "a", "e" }, result[1]);
        Assert.Equal(new[] { "bb", "cc" }, result[2]);
        Assert.Equal(new[] { "ddd" }, result[3]);
    }

    [Fact]
    public void TopNFrequent_orders_by_frequency_then_first_appearance()
    {
        var result = _sut.TopNFrequent(new[] { "a", "b", "a", "c", "b", "a" }, 2);
        Assert.Equal(new[] { "a", "b" }, result);
    }

    [Fact]
    public void TopNFrequent_returns_all_when_n_exceeds_distinct_count()
    {
        var result = _sut.TopNFrequent(new[] { "x", "y", "x" }, 10);
        Assert.Equal(new[] { "x", "y" }, result);
    }
}
