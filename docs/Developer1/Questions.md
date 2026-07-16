# Developer 1 — Verbal / Conceptual Questions

For a junior developer. Keep it supportive — you're gauging fundamentals and
how they think, not trying to stump them. Expected talking points are noted for
the interviewer.

---

## Language fundamentals

**1. Value vs. reference.**
What's the difference between `int` and `string` when you pass them to a method?
- *Looking for:* basic grasp that some types copy the value and others share a
  reference. Don't expect precise stack/heap detail at this level.

**2. `for` vs. `foreach`.**
When would you use each?
- *Looking for:* `foreach` to iterate a collection simply; `for` when you need
  the index or to control stepping.

**3. `null`.**
What is `null` and what happens if you call a method on a null reference?
- *Looking for:* absence of a value; `NullReferenceException`; awareness of null
  checks or `?.`.

**4. Collections.**
What's the difference between an array and a `List<T>`?
- *Looking for:* arrays are fixed size; `List<T>` grows; `List<T>` has Add/Remove.

**5. Exceptions.**
What does `try/catch` do? Give an example of when you'd use it.
- *Looking for:* handle errors without crashing; e.g. parsing user input, file
  or network access.

---

## SQL

**6. What does `SELECT * FROM Customers WHERE City = 'Seattle'` return?**
- *Looking for:* all columns for rows where city is Seattle; understands WHERE
  filters rows.

**7. `ORDER BY`.**
How do you sort results, and how do you reverse the order?
- *Looking for:* `ORDER BY column`, `DESC` for descending.

**8. `COUNT(*)`.**
What does it do?
- *Looking for:* counts rows; often paired with WHERE.

**9. Primary key.**
What is it and why does a table have one?
- *Looking for:* uniquely identifies each row; no duplicates/nulls.

---

## Working style

**10. Getting unstuck.**
You're stuck on a bug for 30 minutes. What do you do?
- *Looking for:* read the error, print/log, search docs, ask a teammate — a
  reasonable, non-passive process.

**11. Version control.**
What is a commit? Why commit often?
- *Looking for:* a saved snapshot; smaller commits are easier to review and
  revert.

**12. Testing.**
Why write tests at all?
- *Looking for:* catch regressions, confidence to change code, document intent.

---

## .NET & SQL — more concepts

**13. Dependency injection.**
You'll see classes that take their dependencies as constructor parameters
instead of creating them with `new`. What's the benefit? (You may have heard of
transient, scoped, and singleton — what do those roughly mean?)
- *Looking for:* passing dependencies in makes code easier to test (swap in a
  fake) and reuse. A rough grasp is fine: a service can be created every time
  it's asked for (transient), once per web request (scoped), or once for the
  whole app (singleton). Don't expect precise rules at this level.

**14. Interfaces.**
What is an interface, and why would you use one?
- *Looking for:* a contract — a list of methods a class promises to provide;
  lets you swap implementations and write tests against the contract.

**15. `UPDATE` without a `WHERE`.**
What does `UPDATE Customers SET City = 'Seattle'` do if you forget the `WHERE`?
- *Looking for:* it updates **every** row. Awareness that `UPDATE`/`DELETE`
  without a `WHERE` hits the whole table; safe habits like selecting first or
  wrapping the change in a transaction.

---

## Code review (hands-on)

Show the candidate this snippet and ask: *"What issues do you see, and how would
you fix them?"* Keep it supportive — you're checking whether they can read code
and spot obvious bugs, not whether they find every subtle one. Reading the code
aloud and reasoning through it out loud is good signal at this level.

**16. Review this method.**
```csharp
public decimal GetDiscount(Customer customer, decimal orderTotal)
{
    decimal discount = 0;
    if (customer.IsVip = true)
        discount = orderTotal * 0.1;

    if (orderTotal > 100)
        discount = discount + 5;

    return discount;
}
```
- *Looking for (should catch):*
  - `customer.IsVip = true` is an **assignment**, not a comparison — it should be
    `if (customer.IsVip)` (or `== true`). As written it always runs and even
    overwrites the flag.
  - `orderTotal * 0.1` mixes `decimal` and `double` — `0.1` needs to be `0.1m`
    or the code won't compile.
- *Looking for (nice to have):*
  - No null check on `customer` → `NullReferenceException` if it's null.
  - Magic numbers (`0.1`, `100`, `5`) would be clearer as named constants.
- *Follow-up:* "How would you test this method?" — expect a couple of concrete
  cases (VIP vs. not, total above/below 100) with expected outputs.

**17. Review this loop.**
```csharp
public bool ContainsSeattle(string[] cities)
{
    for (int i = 0; i <= cities.Length; i++)
    {
        if (cities[i] == "seattle")
            return true;
    }
    return false;
}
```
- *Looking for (should catch):*
  - `i <= cities.Length` is an **off-by-one** — the last iteration reads
    `cities[Length]` and throws `IndexOutOfRangeException`. Should be `<`.
  - The comparison is **case-sensitive**: `"seattle"` won't match `"Seattle"`.
    Use a case-insensitive compare (e.g. `StringComparison.OrdinalIgnoreCase`).
- *Looking for (nice to have):*
  - No null check on `cities` → `NullReferenceException`.
  - `Array.Contains` / LINQ `Any` would be simpler than a manual loop.
- *Follow-up:* "What input would make this crash?" (any non-empty array reaches
  the out-of-bounds index).

**18. Review this error handling.**
```csharp
public int Divide(int a, int b)
{
    try
    {
        return a / b;
    }
    catch (Exception)
    {
        return 0;
    }
}
```
- *Looking for (should catch):*
  - Catching every exception and **returning 0 hides the real problem** — a
    divide-by-zero silently looks like a valid answer of 0.
  - Better to check `if (b == 0)` up front and decide what should happen (throw a
    clear error, or return a sentinel the caller understands).
- *Looking for (nice to have):*
  - `catch (Exception)` is too broad — catch only what you can actually handle.
- *Follow-up:* "If `b` is 0, what *should* happen?" — there's no single right
  answer; look for them reasoning about the caller's needs.
