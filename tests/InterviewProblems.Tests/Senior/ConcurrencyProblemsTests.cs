using InterviewProblems.Senior;

namespace InterviewProblems.Tests.Senior;

[Trait("Level", "Senior")]
[Trait("Category", "Concurrency")]
public class ConcurrencyProblemsTests
{
    private readonly ConcurrencyProblems _sut = new();

    [Fact]
    public async Task MapWithConcurrencyLimit_preserves_order_and_respects_limit()
    {
        var source = Enumerable.Range(1, 20).ToList();
        var current = 0;
        var maxObserved = 0;

        var results = await _sut.MapWithConcurrencyLimitAsync(source, maxConcurrency: 4, async n =>
        {
            var running = Interlocked.Increment(ref current);
            InterlockedMax(ref maxObserved, running);
            await Task.Delay(10);
            Interlocked.Decrement(ref current);
            return n * 2;
        });

        Assert.Equal(source.Select(n => n * 2), results);
        Assert.True(maxObserved <= 4, $"Concurrency limit exceeded: observed {maxObserved}.");
    }

    [Fact]
    public async Task MapWithConcurrencyLimit_throws_for_invalid_limit()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.MapWithConcurrencyLimitAsync(new[] { 1 }, 0, n => Task.FromResult(n)));
    }

    [Fact]
    public void SumConcurrently_returns_correct_total()
    {
        var numbers = Enumerable.Range(1, 1000).Select(n => (long)n).ToList();
        Assert.Equal(500500L, _sut.SumConcurrently(numbers));
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        do
        {
            current = Volatile.Read(ref target);
            if (value <= current)
            {
                return;
            }
        }
        while (Interlocked.CompareExchange(ref target, value, current) != current);
    }
}
