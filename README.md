# Technical Interview

A self-contained coding interview harness for .NET / SQL developers. Candidates
implement a set of method stubs; a suite of xUnit tests tells them (and you)
exactly what passes. SQL problems run against a real, seeded SQLite database
that is rebuilt in memory for every test.

Three levels are included: **Developer 1** (junior), **Developer 2** (mid), and
**Senior**.

**Stack:** .NET 8 (C#) · xUnit · SQLite (`Microsoft.Data.Sqlite`)

## Layout

```
TechAssess.slnx
├── src/
│   ├── InterviewProblems/                 # The problem stubs the candidate implements
│   │   ├── Developer1/                     #   junior: fundamentals + basic SQL
│   │   ├── Developer2/                     #   mid: strings, collections, algorithms, async, SQL joins
│   │   ├── Senior/                         #   senior: concurrency, LRU cache, algorithms, advanced SQL
│   │   ├── Data/                           #   schema.sql + seed.sql (embedded resources)
│   │   └── Infrastructure/                 #   InterviewDatabase: builds the seeded in-memory DB
│   ├── InterviewProblems.Web/              # Blazor coding console (Monaco + Roslyn IntelliSense)
│   └── InterviewProblems.Runner/           # Interactive SQL explorer (dotnet run)
├── tests/InterviewProblems.Tests/          # Validates the candidate's solutions
│   ├── Developer1/  Developer2/  Senior/
│   └── Common/                             # Shared test DB helper
└── docs/
    ├── Developer1/  Developer2/  Senior/   # Questions.md + AnswerKey.md per level
```

## Running the interview

Give the candidate everything **except** the `docs/**/AnswerKey.md` files.

Build and run all tests:

```powershell
dotnet build
dotnet test
```

Every test starts red (the stubs throw `NotImplementedException`). The
candidate's job is to make them green. Scope the run by level or topic:

```powershell
# One level
dotnet test --filter "Level=Developer1"
dotnet test --filter "Level=Developer2"
dotnet test --filter "Level=Senior"

# One topic across levels
dotnet test --filter "Category=SQL"
dotnet test --filter "Category=Async"

# One problem class
dotnet test --filter "FullyQualifiedName~LruCacheTests"
```

## Web coding console (Blazor)

A browser-based coding console lets candidates implement the problems with real
**IntelliSense** and run the tests without a local IDE.

```powershell
dotnet run --project src/InterviewProblems.Web
```

Then open the printed URL (e.g. `http://localhost:5237`). Candidates:

1. Pick a problem from the sidebar (grouped by Developer 1 / 2 / Senior).
2. Implement the stub in a Monaco editor with **Roslyn-powered** completions,
   signature-aware member lists, and live red-squiggle diagnostics.
3. Click **Run tests** — the code is compiled in-memory with Roslyn and graded
   against the exact same xUnit tests used on the command line. Each `[Fact]` /
   `[Theory]` case is reported as PASS/FAIL with the real assert message.
4. **Reset to stub** restores the starting point at any time.

SQL problems run against the same seeded in-memory SQLite database as the test
suite. Nothing the candidate writes is persisted to disk — each run compiles into
a throwaway assembly loaded in a collectible context.

> **Stack:** .NET 8 Blazor Server · Monaco Editor · Roslyn
> (`Microsoft.CodeAnalysis.CSharp.Features`) for both IntelliSense and grading.

### Hosting under a shared web server

The web app supports both a host root (`https://example.com/`) and an IIS/Azure
virtual application (`https://example.com/TechAssess/`). For a virtual
application, add an application setting named `PathBase` with the virtual path,
for example `/TechAssess`, and browse to that path. Leave `PathBase` unset when
the app owns the host root. Navigation and static assets are relative to this
setting, so they do not escape into another application hosted on the same
server.

## Interactive SQL explorer

Let candidates experiment with the database live — results print as a table:

```powershell
dotnet run --project src/InterviewProblems.Runner
```

```
sql> SELECT Name, City FROM Customers WHERE IsActive = 1;
Name  | City
------+---------
Alice | Seattle
Bob   | Portland
...
Commands:  \schema   \tables   \help   \q
```

## What's covered

| Level       | File(s)                                 | Skills exercised                                        |
|-------------|-----------------------------------------|---------------------------------------------------------|
| Developer 1 | `BasicsProblems`, `SqlBasicsProblems`   | Loops, conditionals, strings; SELECT / WHERE / ORDER BY |
| Developer 2 | `StringProblems`, `CollectionProblems`, `AlgorithmProblems`, `AsyncProblems`, `SqlProblems` | Dictionaries, LINQ, generics, Big-O, async/await, joins + aggregation |
| Senior      | `ConcurrencyProblems`, `LruCache`, `AlgorithmProblems`, `AdvancedSqlProblems` | Bounded concurrency, thread safety, cache design, window functions + CTEs |

Every problem has a verbal companion in `docs/<level>/Questions.md` and a
reference solution in `docs/<level>/AnswerKey.md`.

## Suggested format

| Level       | Duration   | Focus                                                         |
|-------------|------------|--------------------------------------------------------------|
| Developer 1 | 45–60 min  | Fundamentals + basic SQL, lots of guidance, watch them think |
| Developer 2 | 60–75 min  | Coding + async + SQL joins, some independence expected       |
| Senior      | 75–90 min  | Concurrency + design + advanced SQL, deep trade-off discussion |

## Adding a new problem

1. Add a stub method (with an XML-doc spec) to a class under `src/InterviewProblems/<level>`.
2. Add a test in the matching `tests/.../<level>/*Tests.cs` file, tagged with
   `[Trait("Level", ...)]` and `[Trait("Category", ...)]`.
3. Add the reference solution to `docs/<level>/AnswerKey.md`.
4. Temporarily implement the stub and run `dotnet test` to confirm the test is
   correct, then revert the stub.

The SQL data set is shared across all levels — see
[src/InterviewProblems/Data/schema.sql](src/InterviewProblems/Data/schema.sql)
and [seed.sql](src/InterviewProblems/Data/seed.sql).
