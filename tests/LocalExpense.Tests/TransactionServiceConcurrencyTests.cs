using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class TransactionServiceConcurrencyTests : IDisposable
{
    private readonly TestFileDbFactory _factory = new();
    private readonly TransactionService _service;

    public TransactionServiceConcurrencyTests() => _service = new TransactionService(_factory);

    public void Dispose() => _factory.Dispose();

    private static readonly DateOnly Oct1 = new(2026, 10, 1);
    private static readonly DateOnly Oct31 = new(2026, 10, 31);

    private static Transaction New(long amount, string category = "Food") =>
        new() { Date = new DateOnly(2026, 10, 15), AmountMinor = amount, Category = category };

    // Task.Run forces genuinely parallel threads; the SQLite calls are largely synchronous
    // underneath, so plain async calls could otherwise run one after another.
    [Fact]
    public async Task ConcurrentAdds_AllSucceedWithDistinctIdsAndNothingLost()
    {
        const int count = 50;

        var saved = await Task.WhenAll(Enumerable.Range(1, count)
            .Select(i => Task.Run(() => _service.AddAsync(New(i)))));

        Assert.Equal(count, saved.Select(t => t.Id).Distinct().Count());
        Assert.Equal(count, (await _service.GetAllAsync()).Count);
        Assert.Equal(count * (count + 1) / 2, await _service.GetTotalAsync(Oct1, Oct31));   // 1 + 2 + … + 50
    }

    [Fact]
    public async Task ReadsDuringWrites_NeverThrowAndAlwaysSeeAConsistentState()
    {
        const int writes = 40;

        var writers = Enumerable.Range(1, writes)
            .Select(_ => Task.Run(() => _service.AddAsync(New(100))));
        var readers = Enumerable.Range(1, writes)
            .Select(_ => Task.Run(async () =>
            {
                var rows = await _service.GetAllAsync();
                var total = await _service.GetTotalAsync(Oct1, Oct31);
                Assert.True(rows.Count <= writes);
                Assert.Equal(0, total % 100);   // only whole 100-cent rows can ever have been committed
            }));

        await Task.WhenAll(writers.Concat<Task>(readers));

        Assert.Equal(writes * 100, await _service.GetTotalAsync(Oct1, Oct31));
    }

    [Fact]
    public async Task ConcurrentUpdatesToDifferentRows_AreAllApplied()
    {
        var rows = await Task.WhenAll(Enumerable.Range(1, 20)
            .Select(_ => _service.AddAsync(New(1))));

        await Task.WhenAll(rows.Select(r => Task.Run(() => _service.UpdateAsync(new Transaction
        {
            Id = r.Id,
            Date = r.Date,
            AmountMinor = r.Id * 10,
            Category = "Updated",
        }))));

        var all = await _service.GetAllAsync();
        Assert.All(all, t =>
        {
            Assert.Equal("Updated", t.Category);
            Assert.Equal(t.Id * 10, t.AmountMinor);
        });
    }

    [Fact]
    public async Task ConcurrentUpdatesToTheSameRow_LeaveOneOfTheWrittenValues()
    {
        var row = await _service.AddAsync(New(1));
        var candidates = Enumerable.Range(1, 20).Select(i => (long)i * 1000).ToArray();

        var results = await Task.WhenAll(candidates.Select(amount => Task.Run(() => _service.UpdateAsync(new Transaction
        {
            Id = row.Id,
            Date = row.Date,
            AmountMinor = amount,
            Category = "Food",
        }))));

        Assert.All(results, Assert.True);
        var final = await _service.GetByIdAsync(row.Id);
        Assert.Contains(final!.AmountMinor, candidates);   // last writer wins; never a mix, never the original
    }

    [Fact]
    public async Task DeletingTheSameRowConcurrently_SucceedsExactlyOnce()
    {
        var row = await _service.AddAsync(New(1));

        var results = await Task.WhenAll(Enumerable.Range(1, 10)
            .Select(_ => Task.Run(() => _service.DeleteAsync(row.Id))));

        Assert.Equal(1, results.Count(deleted => deleted));
        Assert.Null(await _service.GetByIdAsync(row.Id));
    }
}
