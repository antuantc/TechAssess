# Senior — Verbal / Conceptual Questions

For a senior engineer. Push on trade-offs, failure modes, and judgment — not
trivia. The best signal comes from follow-ups ("what breaks at scale?",
"how would you test that?"). Expected talking points noted for the interviewer.

---

## Concurrency & async

**1. `Task.WhenAll` vs. `Parallel.ForEach`.**
When do you reach for each?
- *Looking for:* `WhenAll` for I/O-bound async work (no thread per item);
  `Parallel.ForEach` for CPU-bound work across cores. Mixing them up (blocking
  threads on async I/O) is a common senior-level mistake to probe.

**2. Bounded concurrency.**
You need to call an API for 10,000 items but it allows 5 concurrent requests.
How?
- *Looking for:* `SemaphoreSlim`, `Parallel.ForEachAsync` with
  `MaxDegreeOfParallelism`, or a channel/queue. This mirrors the
  `MapWithConcurrencyLimitAsync` problem.

**3. Race conditions.**
What is one, and how do you make a shared counter safe?
- *Looking for:* interleaved read-modify-write; fix with `Interlocked`, `lock`,
  or partition-then-combine. Awareness that locks have contention cost.

**4. Deadlocks.**
How does `.Result` / `.Wait()` cause a deadlock, and how do you avoid it?
- *Looking for:* blocking on a synchronization context that the continuation
  needs; avoid by going async all the way, or `ConfigureAwait(false)` in
  libraries.

**5. Cancellation.**
How do you make a long-running async operation cancellable?
- *Looking for:* `CancellationToken` threaded through, checked cooperatively,
  `ThrowIfCancellationRequested` / passing it to async calls.

---

## Design & architecture

**6. Cache design.**
Walk me through an LRU cache. What data structures, and why O(1)?
- *Looking for:* hash map + doubly linked list; map for lookup, list for
  recency ordering; move-to-front on access, evict from tail. This mirrors the
  `LruCache` problem. Follow-up: thread safety, TTL, memory bounds.

**7. Caching pitfalls.**
What are the hard problems with caching in a real system?
- *Looking for:* invalidation, staleness, stampede/thundering herd, cache
  coherence across nodes, memory pressure.

**8. SOLID / abstraction judgment.**
When is an abstraction premature?
- *Looking for:* mature take — abstractions have cost; wait for the second or
  third real use; avoid speculative generality.

**9. API / backward compatibility.**
You must change a widely-used public method. How do you avoid breaking callers?
- *Looking for:* additive changes, overloads, `[Obsolete]`, versioning,
  deprecation windows.

---

## SQL & data

**10. Window functions.**
What can `ROW_NUMBER() OVER (...)` or `SUM(...) OVER (...)` do that `GROUP BY`
can't?
- *Looking for:* per-row results alongside aggregates, running totals, ranking,
  "top N per group" — without collapsing rows. Mirrors the advanced SQL problems.

**11. Query performance.**
A query got slow as the table grew. How do you diagnose and fix it?
- *Looking for:* `EXPLAIN`/query plan, missing index, full scan, N+1 from the
  app, over-fetching; add the right index, rewrite, or paginate.

**12. Transaction isolation.**
What problems do isolation levels prevent, and what's the trade-off?
- *Looking for:* dirty/non-repeatable/phantom reads; higher isolation costs
  concurrency (locking/serialization). Optimistic vs. pessimistic concurrency.

**13. N+1 queries.**
What is it, and how do you spot and fix it?
- *Looking for:* one query per row in a loop; fix with a join, batched IN, or
  eager loading. Spot via query logs/APM.

---

## Leadership & judgment

**14. Technical debt.**
How do you decide when to pay it down vs. ship?
- *Looking for:* pragmatic framing — debt as a trade-off tied to business risk,
  not purity; make it visible, pay down where it slows the team most.

**15. Mentoring / disagreement.**
A junior's PR works but you'd have done it differently. What do you do?
- *Looking for:* distinguishes "wrong" from "not how I'd do it"; teaches through
  questions; picks battles; respects their ownership.

---

## .NET & SQL — more concepts

**16. DI lifetimes & the captive dependency.**
Explain transient, scoped, and singleton. What's the captive dependency problem,
and why is a singleton `DbContext` dangerous?
- *Looking for:* precise semantics — transient per resolve, scoped per
  request/scope, singleton per app. **Captive dependency:** a longer-lived
  service that captures a shorter-lived one, pinning it past its intended
  lifetime. `DbContext` is not thread-safe and holds change-tracking state, so a
  singleton one corrupts under concurrency. Bonus: container scope validation,
  and that transient `IDisposable`s resolved from the root scope live until app
  shutdown.

**17. `ConfigureAwait(false)`.**
What does it do, and when does it matter?
- *Looking for:* it tells the continuation not to capture the current
  synchronization context. Matters in **library** code to avoid deadlocks (the
  classic `.Result` on a captured context) and to shave overhead. Largely moot in
  ASP.NET Core, which has no sync context — a strong candidate knows the nuance.

**18. Clustered vs. non-clustered indexes.**
What's the difference, and why does it matter for query performance?
- *Looking for:* clustered = the physical row order (one per table, usually the
  PK); non-clustered = a separate structure pointing back to the rows. Awareness
  of key lookups and **covering indexes** (`INCLUDE`) to avoid them.

**19. Database deadlocks.**
What causes one, and how do you diagnose and prevent it?
- *Looking for:* two transactions each holding a lock the other needs. Prevent
  with consistent access ordering, shorter transactions, the right isolation
  level, and retrying the deadlock victim; diagnose via the deadlock graph.

---

## Code review (hands-on)

Show the snippet and ask: *"This caches users in memory. What could go wrong in
production?"* This is a judgment probe — the code compiles and works for a single
request. The signal is whether they see the concurrency, memory, and staleness
failure modes at scale, and can weigh the fixes.

**20. Review this cache.**
```csharp
private static Dictionary<string, User> _cache = new();

public async Task<User> GetUserAsync(string id)
{
    if (_cache.ContainsKey(id))
        return _cache[id];

    var user = await _repo.LoadAsync(id);
    _cache[id] = user;
    return user;
}
```
- *Looking for (must catch):*
  - **`Dictionary` is not thread-safe.** Concurrent access can corrupt it or
    throw, and the `static` field is shared across every request. Use
    `ConcurrentDictionary`, a lock, or a real cache (`IMemoryCache`).
  - **Unbounded growth / memory leak** — nothing ever evicts entries; needs a
    size bound or TTL.
  - **No invalidation** — once cached, a user is stale forever after an update.
- *Looking for (strong signal):*
  - **Cache stampede** — on a cold miss, many concurrent callers all hit
    `_repo.LoadAsync`. Mitigate with per-key locking / `GetOrCreateAsync` /
    a cached `Lazy<Task<User>>`.
  - `ContainsKey` then the indexer is a **double lookup** and a TOCTOU race —
    prefer `TryGetValue`.
  - Caches `null` / faulted results if `LoadAsync` returns null or throws —
    the negative-caching policy should be deliberate.
  - No `CancellationToken` threaded through.
- *Follow-up:* "How does this behave across two servers behind a load balancer?"
  (per-node caches diverge → distributed cache, or accept eventual staleness.)

**21. Review this transfer.**
```csharp
public async Task Transfer(int fromId, int toId, decimal amount)
{
    var from = await _db.Accounts.FirstAsync(a => a.Id == fromId);
    var to   = await _db.Accounts.FirstAsync(a => a.Id == toId);
    from.Balance -= amount;
    await _db.SaveChangesAsync();
    to.Balance   += amount;
    await _db.SaveChangesAsync();
}
```
- *Looking for (must catch):*
  - **Two saves = not atomic.** A crash between them debits one account without
    crediting the other — money disappears. Both updates must be one transaction
    (a single `SaveChangesAsync`, or an explicit transaction).
  - **No validation** — `amount > 0`, sufficient funds (overdraft), `fromId != toId`.
  - `FirstAsync` **throws** if an account is missing → unhandled failure.
- *Looking for (strong signal):*
  - **Lost update / double-spend under concurrency** — two transfers reading the
    same balance clobber each other. Needs optimistic concurrency (`rowversion`)
    or a DB-side atomic update (`UPDATE ... SET balance = balance - @amount`).
  - Consistent lock ordering to avoid deadlocks when many transfers run at once.
- *Follow-up:* "Two transfers hit the same account at the same instant — walk me
  through what each one reads and writes." (exposes the race concretely).

**22. Review this worker.**
```csharp
public async void ProcessQueue()
{
    while (true)
    {
        var msg = await _queue.DequeueAsync();
        await _handler.HandleAsync(msg);
    }
}
```
- *Looking for (must catch):*
  - **`async void`** — it can't be awaited and any exception is thrown on the
    sync-context / crashes the process. Should return `Task` (ideally a
    `BackgroundService.ExecuteAsync`).
  - **One thrown exception kills the loop** — a single bad message stops all
    processing. Wrap the body in try/catch, log, and decide on retry / dead-letter
    (poison-message) handling.
- *Looking for (strong signal):*
  - `while (true)` with **no `CancellationToken`** — no graceful shutdown; the loop
    can't be stopped and blocks the host from stopping cleanly.
  - No back-off when the queue is empty or the handler keeps failing (hot loop /
    retry storm).
- *Follow-up:* "A message throws on line two at 3am. What's the state of the
  service at 4am?" (dead — the unobserved `async void` exception took it down).
