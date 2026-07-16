namespace InterviewProblems.Senior;

/// <summary>
/// Senior — Design a fixed-capacity Least-Recently-Used (LRU) cache.
///
/// Implement this class so that the tests in
/// <c>tests/InterviewProblems.Tests/Senior/LruCacheTests.cs</c> pass.
/// Both <see cref="TryGet"/> and <see cref="Put"/> should run in O(1) time.
/// A "use" is any successful <see cref="TryGet"/> or any <see cref="Put"/>.
/// When the cache is full and a new key is added, evict the least recently
/// used entry.
/// </summary>
public class LruCache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Create a cache that holds at most <paramref name="capacity"/> entries.
    /// Throw <see cref="ArgumentOutOfRangeException"/> if capacity &lt; 1.
    /// </summary>
    public LruCache(int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        // TODO: initialize the data structures you need for O(1) access + eviction.
    }

    /// <summary>The number of entries currently held.</summary>
    public int Count => throw new NotImplementedException();

    /// <summary>
    /// Try to get the value for <paramref name="key"/>. Returns <c>true</c> and
    /// marks the key as most-recently-used if present; otherwise returns
    /// <c>false</c>.
    /// </summary>
    public bool TryGet(TKey key, out TValue value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Insert or update the value for <paramref name="key"/>, marking it as
    /// most-recently-used. If inserting exceeds capacity, evict the
    /// least-recently-used entry first.
    /// </summary>
    public void Put(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }
}
