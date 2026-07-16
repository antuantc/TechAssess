using InterviewProblems.Senior;

namespace InterviewProblems.Tests.Senior;

[Trait("Level", "Senior")]
[Trait("Category", "DataStructures")]
public class LruCacheTests
{
    [Fact]
    public void Constructor_rejects_non_positive_capacity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LruCache<int, string>(0));
    }

    [Fact]
    public void Evicts_least_recently_used_entry()
    {
        var cache = new LruCache<int, string>(2);
        cache.Put(1, "a");
        cache.Put(2, "b");

        // Touch key 1 so it becomes most-recently-used.
        Assert.True(cache.TryGet(1, out var v1));
        Assert.Equal("a", v1);

        // Adding a third entry should evict key 2 (least recently used).
        cache.Put(3, "c");

        Assert.False(cache.TryGet(2, out _));
        Assert.True(cache.TryGet(1, out _));
        Assert.True(cache.TryGet(3, out var v3));
        Assert.Equal("c", v3);
        Assert.Equal(2, cache.Count);
    }

    [Fact]
    public void Updating_existing_key_refreshes_recency_and_value()
    {
        var cache = new LruCache<int, int>(2);
        cache.Put(1, 1);
        cache.Put(2, 2);

        // Update key 1 — it becomes most recent and its value changes.
        cache.Put(1, 10);

        // Adding key 3 should evict key 2, not key 1.
        cache.Put(3, 3);

        Assert.False(cache.TryGet(2, out _));
        Assert.True(cache.TryGet(1, out var v1));
        Assert.Equal(10, v1);
    }

    [Fact]
    public void Missing_key_returns_false()
    {
        var cache = new LruCache<string, int>(2);
        Assert.False(cache.TryGet("nope", out var value));
        Assert.Equal(0, value);
    }
}
