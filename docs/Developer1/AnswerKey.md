# Developer 1 — Answer Key (INTERVIEWER ONLY)

> Do **not** share this file with the candidate. Reference solutions that make
> every Developer 1 test green. Multiple valid approaches exist.

## BasicsProblems

```csharp
public int Sum(IEnumerable<int> numbers) => numbers.Sum();

public int Max(IReadOnlyList<int> numbers)
{
    if (numbers.Count == 0) throw new ArgumentException("List is empty.", nameof(numbers));
    return numbers.Max();
}

public string FizzBuzz(int number)
{
    if (number % 15 == 0) return "FizzBuzz";
    if (number % 3 == 0) return "Fizz";
    if (number % 5 == 0) return "Buzz";
    return number.ToString();
}

public int CountVowels(string input)
{
    if (string.IsNullOrEmpty(input)) return 0;
    return input.Count(c => "aeiouAEIOU".Contains(c));
}

public string ReverseString(string input)
{
    if (string.IsNullOrEmpty(input)) return string.Empty;
    var chars = input.ToCharArray();
    Array.Reverse(chars);
    return new string(chars);
}

public bool IsPalindrome(string input)
{
    var cleaned = new string((input ?? string.Empty)
        .Where(c => !char.IsWhiteSpace(c))
        .Select(char.ToLowerInvariant)
        .ToArray());
    return cleaned.SequenceEqual(cleaned.Reverse());
}
```

**Discussion:** `FizzBuzz` — the `% 15` (or `%3 && %5`) case must come first,
otherwise 15 prints "Fizz". A candidate who writes the checks in the wrong order
is the classic FizzBuzz tell. Loop-based reversal (building a string backwards)
is equally acceptable at this level.

## SqlBasicsProblems

```sql
-- AllCustomerNamesAlphabetical → Alice, Bob, Carol, Dave, Eve
SELECT Name FROM Customers ORDER BY Name;

-- CustomersInPortland → Bob, Eve
SELECT Name FROM Customers WHERE City = 'Portland' ORDER BY Name;

-- ProductsCheaperThan20 → Gizmo, Widget, eBook
SELECT Name FROM Products WHERE Price < 20 ORDER BY Price;

-- TotalNumberOfOrders → 5
SELECT COUNT(*) FROM Orders;
```

**Discussion:** watch that they use `WHERE` for filtering (not fetching
everything and filtering in C#), and that string literals use single quotes.
