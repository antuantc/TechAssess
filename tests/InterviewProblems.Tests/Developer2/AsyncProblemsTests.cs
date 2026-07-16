using InterviewProblems.Developer2;

namespace InterviewProblems.Tests.Developer2;

[Trait("Level", "Developer2")]
[Trait("Category", "Async")]
public class AsyncProblemsTests
{
    private readonly AsyncProblems _sut = new();

    [Fact]
    public async Task SumAsync_awaits_and_sums_all_results()
    {
        var tasks = new[]
        {
            Task.FromResult(1L),
            Task.FromResult(2L),
            Task.Run(async () => { await Task.Delay(20); return 3L; })
        };

        Assert.Equal(6L, await _sut.SumAsync(tasks));
    }

    [Fact]
    public async Task FirstToCompleteAsync_returns_the_fastest_result()
    {
        var tasks = new[]
        {
            Task.Run(async () => { await Task.Delay(5000); return 1; }),
            Task.FromResult(42)
        };

        Assert.Equal(42, await _sut.FirstToCompleteAsync(tasks));
    }

    [Fact]
    public async Task WithRetryAsync_succeeds_after_transient_failures()
    {
        var attempts = 0;
        var result = await _sut.WithRetryAsync(() =>
        {
            attempts++;
            if (attempts < 3)
            {
                throw new InvalidOperationException("transient");
            }

            return Task.FromResult("ok");
        }, maxAttempts: 3);

        Assert.Equal("ok", result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task WithRetryAsync_rethrows_after_exhausting_attempts()
    {
        var attempts = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.WithRetryAsync<string>(() =>
            {
                attempts++;
                throw new InvalidOperationException("always fails");
            }, maxAttempts: 2));

        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task WithTimeoutAsync_returns_result_when_fast()
    {
        var result = await _sut.WithTimeoutAsync(Task.FromResult(7), TimeSpan.FromSeconds(1));
        Assert.Equal(7, result);
    }

    [Fact]
    public async Task WithTimeoutAsync_throws_when_slow()
    {
        var slow = Task.Run(async () => { await Task.Delay(5000); return 1; });
        await Assert.ThrowsAsync<TimeoutException>(() =>
            _sut.WithTimeoutAsync(slow, TimeSpan.FromMilliseconds(50)));
    }
}
