# Senior — Answer Key (INTERVIEWER ONLY)

> Do **not** share this file with the candidate. Reference solutions that make
> every Senior test green. Multiple valid approaches exist — reward correct,
> well-reasoned solutions even if they differ.

---

## ConcurrencyProblems

```csharp
public async Task<IReadOnlyList<TResult>> MapWithConcurrencyLimitAsync<TSource, TResult>(
    IReadOnlyList<TSource> source, int maxConcurrency, Func<TSource, Task<TResult>> operation)
{
    if (maxConcurrency < 1) throw new ArgumentOutOfRangeException(nameof(maxConcurrency));
    var results = new TResult[source.Count];
    using var gate = new SemaphoreSlim(maxConcurrency);
    var tasks = source.Select(async (item, index) =>
    {
        await gate.WaitAsync();
        try { results[index] = await operation(item); }
        finally { gate.Release(); }
    });
    await Task.WhenAll(tasks);
    return results;
}

public long SumConcurrently(IEnumerable<long> numbers)
{
    long total = 0;
    Parallel.ForEach(numbers, n => Interlocked.Add(ref total, n));
    return total;
}
```

**Discussion:** the key insight in `MapWithConcurrencyLimitAsync` is writing to
`results[index]` (order preserved) rather than adding to a shared list (order
lost, and not thread-safe). `.NET 6+` also has `Parallel.ForEachAsync`.
`SumConcurrently` — a plain `total += n` across threads loses updates;
`Interlocked.Add` or a `lock` fixes it. A partition-local sum + combine is the
performance-minded answer.

## LruCache

```csharp
public class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _map;
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new();

    public LruCache(int capacity)
    {
        if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
        _map = new Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>>(capacity);
    }

    public int Count => _map.Count;

    public bool TryGet(TKey key, out TValue value)
    {
        if (_map.TryGetValue(key, out var node))
        {
            _order.Remove(node);
            _order.AddFirst(node);      // most-recently-used at the front
            value = node.Value.Value;
            return true;
        }
        value = default!;
        return false;
    }

    public void Put(TKey key, TValue value)
    {
        if (_map.TryGetValue(key, out var existing))
        {
            _order.Remove(existing);
        }
        else if (_map.Count >= _capacity)
        {
            var lru = _order.Last!;      // least-recently-used at the tail
            _order.RemoveLast();
            _map.Remove(lru.Value.Key);
        }

        var node = new LinkedListNode<(TKey Key, TValue Value)>((key, value));
        _order.AddFirst(node);
        _map[key] = node;
    }
}
```

**Discussion:** dictionary → O(1) lookup; doubly linked list → O(1) move-to-front
and O(1) tail eviction. Storing the *node* in the dictionary is what makes
`_order.Remove(node)` O(1) (removing by value would be O(n)). Storing the key
inside the node lets eviction remove the map entry. Follow-ups: thread safety
(lock or `ConcurrentDictionary` + care), TTL, and `.NET`'s built-in
`MemoryCache`.

## AlgorithmProblems

```csharp
public IList<IList<string>> GroupAnagrams(IEnumerable<string> words)
{
    var map = new Dictionary<string, IList<string>>();
    var order = new List<string>();
    foreach (var word in words)
    {
        var key = new string(word.OrderBy(c => c).ToArray());
        if (!map.TryGetValue(key, out var group))
        {
            group = new List<string>();
            map[key] = group;
            order.Add(key);
        }
        group.Add(word);
    }
    return order.Select(k => map[k]).ToList();
}

public int[][] MergeIntervals(int[][] intervals)
{
    if (intervals.Length == 0) return Array.Empty<int[]>();
    var sorted = intervals.OrderBy(i => i[0]).ToArray();
    var merged = new List<int[]>();
    var current = new[] { sorted[0][0], sorted[0][1] };
    foreach (var interval in sorted.Skip(1))
    {
        if (interval[0] <= current[1])
            current[1] = Math.Max(current[1], interval[1]);
        else { merged.Add(current); current = new[] { interval[0], interval[1] }; }
    }
    merged.Add(current);
    return merged.ToArray();
}
```

**Discussion:** `GroupAnagrams` — sorted-characters key is O(k log k) per word;
a 26-letter count key is O(k) and worth mentioning. `MergeIntervals` — sorting by
start is the crux; the merge is a single pass. Watch the touching case
(`[4,5]` merges with `[1,4]`): the test uses `<=`, not `<`.

---

## AdvancedSqlProblems

```sql
-- CustomerRevenueRanking → (Bob,1),(Dave,2),(Alice,3)
SELECT c.Name,
       ROW_NUMBER() OVER (ORDER BY SUM(oi.Quantity * oi.UnitPrice) DESC) AS Rank
FROM Customers c
JOIN Orders o ON o.CustomerId = c.Id AND o.Status = 'Completed'
JOIN OrderItems oi ON oi.OrderId = o.Id
GROUP BY c.Id, c.Name
ORDER BY Rank;

-- SecondHighestProductPrice → 25
SELECT Price FROM (
    SELECT DISTINCT Price FROM Products ORDER BY Price DESC LIMIT 2
) ORDER BY Price ASC LIMIT 1;

-- RunningMonthlyRevenue → (2024-06,70,70),(2024-07,250,320)
SELECT Month, Revenue, SUM(Revenue) OVER (ORDER BY Month) AS RunningTotal
FROM (
    SELECT strftime('%Y-%m', o.OrderDate) AS Month,
           SUM(oi.Quantity * oi.UnitPrice) AS Revenue
    FROM Orders o
    JOIN OrderItems oi ON oi.OrderId = o.Id
    WHERE o.Status = 'Completed'
    GROUP BY Month
)
ORDER BY Month;

-- OrdersAboveAverageValue → 3, 5
WITH OrderTotals AS (
    SELECT o.Id AS OrderId, SUM(oi.Quantity * oi.UnitPrice) AS Total
    FROM Orders o
    JOIN OrderItems oi ON oi.OrderId = o.Id
    WHERE o.Status = 'Completed'
    GROUP BY o.Id
)
SELECT OrderId FROM OrderTotals
WHERE Total > (SELECT AVG(Total) FROM OrderTotals)
ORDER BY OrderId;
```

**Discussion points:**
- `CustomerRevenueRanking` — the window function runs *after* `GROUP BY`, so it
  ranks the aggregated rows. `RANK()` vs `ROW_NUMBER()` vs `DENSE_RANK()` differ
  on ties; ask which they'd use and why.
- `SecondHighestProductPrice` — `DISTINCT` matters if two products share a price;
  `DENSE_RANK() = 2` is an equally good answer.
- `RunningMonthlyRevenue` — the running total is a windowed `SUM` over an ordered
  frame; a self-join is the pre-window-function way (worth mentioning as the
  "old" approach).
- `OrdersAboveAverageValue` — the CTE avoids computing the per-order totals
  twice; the average is a scalar subquery over the same CTE.
