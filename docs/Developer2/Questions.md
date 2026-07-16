# Developer 2 — Verbal / Conceptual Questions

Pair these with the coding exercises. They probe understanding beyond "does the
test pass." Expected talking points are noted for the interviewer. Aim for
reasoning quality over textbook recall.

---

## C# / .NET

**1. Value types vs. reference types.**
When would you use a `struct` over a `class`?
- *Looking for:* stack vs. heap, copy-by-value semantics, small immutable data,
  avoiding allocations; awareness that mutable structs are a footgun.

**2. `IEnumerable<T>` vs. `List<T>`.**
Why return `IEnumerable<T>` from a method? What's the risk?
- *Looking for:* deferred/lazy execution, streaming, abstraction; the risk of
  multiple enumeration and re-running expensive queries. `ToList()` to materialize.

**3. `async`/`await`.**
What does `await` actually do? What is the danger of `async void` and of
`.Result` / `.Wait()`?
- *Looking for:* frees the thread while I/O completes, not multithreading per se;
  `async void` can't be awaited and swallows exceptions; blocking on async can
  deadlock and defeats the purpose.

**4. Exception handling.**
When do you catch an exception vs. let it bubble? What's wrong with
`catch (Exception) { }`?
- *Looking for:* catch only what you can handle; don't swallow; preserve stack
  traces (`throw;` not `throw ex;`); use `finally`/`using` for cleanup.

**5. Dependency injection.**
What problem does DI solve? Difference between transient, scoped, and singleton
lifetimes?
- *Looking for:* decoupling, testability (inject fakes/mocks); lifetime scoping
  and the danger of capturing a scoped service inside a singleton.

**6. `IDisposable` / `using`.**
When and why do you implement `IDisposable`?
- *Looking for:* deterministic release of unmanaged/expensive resources (DB
  connections, files, sockets); `using` guarantees `Dispose` even on exceptions.

---

## SQL / Databases

**7. INNER vs. LEFT JOIN.**
Give an example where choosing the wrong one changes the result.
- *Looking for:* LEFT JOIN keeps unmatched left rows (NULLs on the right);
  e.g. "customers with no orders" needs LEFT JOIN + `IS NULL` (or `NOT IN`).

**8. `WHERE` vs. `HAVING`.**
- *Looking for:* `WHERE` filters rows before grouping; `HAVING` filters after
  aggregation. You can't put `SUM(...) > 100` in `WHERE`.

**9. Indexes.**
What is an index, and what's the trade-off of adding one?
- *Looking for:* speeds up reads/lookups (B-tree), slows writes and uses storage;
  index the columns you filter/join/sort on.

**10. SQL injection.**
How does it happen and how do you prevent it?
- *Looking for:* string-concatenating user input into SQL; prevent with
  parameterized queries / prepared statements — never string interpolation.

**11. Transactions / ACID.**
What does a transaction give you? Name what the ACID letters stand for.
- *Looking for:* all-or-nothing unit of work; Atomicity, Consistency, Isolation,
  Durability; rollback on failure.

---

## Design & Practice

**12. SOLID — pick one.**
Explain one SOLID principle with a concrete example.
- *Looking for:* a real, specific example, not just the definition. Single
  Responsibility and Dependency Inversion are the most common strong answers.

**13. Code review.**
What do you look for when reviewing a teammate's pull request?
- *Looking for:* correctness, readability, tests, edge cases, security, naming;
  a collaborative, non-gatekeeping tone.

**14. Testing.**
What makes a good unit test? What's the difference between a unit and an
integration test?
- *Looking for:* fast, isolated, deterministic, one reason to fail; integration
  tests cross real boundaries (DB, HTTP). The SQL tests here are integration-ish.

**15. Debugging a production issue.**
A specific endpoint is intermittently slow. Walk me through how you'd
investigate.
- *Looking for:* reproduce, measure (logs/metrics/APM), form a hypothesis, isolate
  (DB query? N+1? lock? GC? external call?), verify the fix. Structured, not
  guess-and-check.

---

## .NET & SQL — more concepts

> Lifetimes (transient / scoped / singleton) are defined in **Q5** — this section
> pushes on the pitfalls and adds a few more .NET and SQL topics.

**16. DI lifetime pitfalls.**
What goes wrong if you inject a *scoped* service into a *singleton*? Why is a
singleton `DbContext` a bad idea?
- *Looking for:* the **captive dependency** — the singleton holds the scoped
  service for the app's entire life, so it never gets a fresh instance.
  `DbContext` isn't thread-safe and is meant to be short-lived (scoped), so a
  singleton one causes stale data and concurrency bugs. ASP.NET Core's scope
  validation catches some of this in development.

**17. `Task` vs. `Thread`.**
What's the difference?
- *Looking for:* a `Thread` is an OS thread you manage directly; a `Task` is a
  higher-level unit of work scheduled on the thread pool. Async `Task`s don't
  necessarily tie up a thread while awaiting I/O. Prefer `Task`/`async` over raw
  threads.

**18. `DELETE` vs. `TRUNCATE` vs. `DROP`.**
- *Looking for:* `DELETE` removes rows (can use `WHERE`, is logged, fires
  triggers); `TRUNCATE` fast-empties the whole table (resets identity, minimal
  logging, no `WHERE`); `DROP` removes the table itself — structure and all.

**19. `UNION` vs. `UNION ALL`.**
- *Looking for:* `UNION` removes duplicate rows (extra work to de-dupe);
  `UNION ALL` keeps everything and is faster. Use `UNION ALL` unless you actually
  need de-duplication.

---

## Code review (hands-on)

Show the candidate this snippet and ask: *"This is a controller action that saves
an order. What issues do you see, and how would you improve it?"* Let them drive;
nudge toward the categories they miss. Strong candidates reach atomicity, async,
and input validation without prompting.

**20. Review this controller action.**
```csharp
public async Task<IActionResult> Save(Order order)
{
    if (order.Items.Count > 0)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var customer = _db.Customers.First(c => c.Id == order.CustomerId);
        customer.LastOrderDate = DateTime.Now;
        _db.SaveChanges();

        _logger.LogInformation("Saved order");
    }
    return Ok();
}
```
- *Looking for (correctness / data):*
  - **Two separate saves, not atomic** — if the second fails, the order is saved
    but `LastOrderDate` isn't. Track both changes and call `SaveChangesAsync`
    once (a single `SaveChanges` is already one transaction).
  - `First(...)` **throws** if the customer isn't found → unhandled 500; use
    `FirstOrDefaultAsync` and return `NotFound`.
  - `order.Items` could be **null** → `NullReferenceException` on `.Count`.
- *Looking for (async):*
  - Mixes blocking `SaveChanges()` and sync `First` inside an async method — go
    async all the way (`FirstOrDefaultAsync`, `SaveChangesAsync`) and thread a
    `CancellationToken`.
- *Looking for (data correctness):*
  - `DateTime.Now` → `DateTime.UtcNow` (store UTC to avoid time-zone/DST bugs).
- *Looking for (input / security):*
  - Binding straight to the EF entity allows **over-posting** (client sets fields
    it shouldn't) — bind to a DTO and validate `ModelState`.
- *Looking for (API design):*
  - Empty items silently returns `Ok()` having saved nothing — should be
    `400 BadRequest`; a successful create should return `201 Created`.
- *Looking for (nice to have):*
  - Structured logging with IDs (`"Saved order {OrderId} for customer {CustomerId}"`);
    lost-update race on `LastOrderDate`; no exception handling around the DB calls.
- *Follow-up:* "What happens if the client retries this POST after a timeout?"
  (duplicate orders → idempotency key / unique constraint.)

**21. Review this endpoint.**
```csharp
public IActionResult GetOrderTotal(int customerId)
{
    var orders = _db.Orders.Where(o => o.CustomerId == customerId).ToList();
    decimal total = 0;
    foreach (var o in orders)
    {
        var items = _db.OrderItems.Where(i => i.OrderId == o.Id).ToList();
        total += items.Sum(i => i.Price * i.Quantity);
    }
    return Ok(total);
}
```
- *Looking for (must catch):*
  - **N+1 queries** — one `OrderItems` query per order. Fix with a join / `Include`
    / a single projection, or push the whole sum into one SQL query.
  - The total is **computed in memory** after pulling every row — `SumAsync` (or a
    `GROUP BY`) lets the database do it and returns one number.
- *Looking for (strong signal):*
  - Not async — `ToListAsync` / `SumAsync`, plus a `CancellationToken`.
  - No handling for a customer with no orders (returns 0, which may be fine — or
    should it be `404`? worth a sentence).
- *Follow-up:* "How does this behave for a customer with 10,000 orders?"
  (thousands of round-trips + loading everything into memory).

**22. Review this method.**
```csharp
public async Task Notify(int userId)
{
    try
    {
        var user = _db.Users.Find(userId);
        var client = new HttpClient();
        await client.PostAsync(_url, new StringContent(user.Email));
    }
    catch { }
}
```
- *Looking for (must catch):*
  - **Empty `catch { }` swallows every failure** — network errors, a null `user`,
    everything vanishes silently. At minimum log it; don't hide it.
  - **`new HttpClient()` per call** exhausts sockets under load — use
    `IHttpClientFactory` / a shared client.
- *Looking for (strong signal):*
  - `user` can be **null** (`Find` returns null) → `NullReferenceException`
    (swallowed by the empty catch, so it fails invisibly).
  - Sync `Find` inside an async method — use `FindAsync`; add a `CancellationToken`
    and a timeout.
  - Sending a user's email (PII) with no auth on the request is worth flagging.
- *Follow-up:* "You deploy this and notifications randomly stop working. How would
  you even find out?" (you can't — the empty catch hides it; that's the point).
