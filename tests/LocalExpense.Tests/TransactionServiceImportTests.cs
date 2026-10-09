using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class TransactionServiceImportTests : IDisposable
{
    private readonly TestDbFactory _factory = new();
    private readonly TransactionService _service;

    public TransactionServiceImportTests() => _service = new TransactionService(_factory);

    public void Dispose() => _factory.Dispose();

    private static Transaction Tx(string date, long minor, string category = "Food", string? note = null) => new()
    {
        Date = DateOnly.Parse(date, System.Globalization.CultureInfo.InvariantCulture),
        AmountMinor = minor,
        Category = category,
        Note = note,
    };

    [Fact]
    public async Task AddRange_InsertsEveryRowIncludingRepeatedOnes()
    {
        var added = await _service.AddRangeAsync([Tx("2025-01-01", -350, "Coffee"), Tx("2025-01-01", -350, "Coffee"), Tx("2025-01-02", 100)]);

        Assert.Equal(3, added);
        Assert.Equal(3, (await _service.GetAllAsync()).Count);
    }

    [Fact]
    public async Task AddRange_KeepsWhatIsAlreadyStored()
    {
        await _service.AddAsync(Tx("2025-01-01", -350, "Coffee"));

        await _service.AddRangeAsync([Tx("2025-01-01", -350, "Coffee")]);

        Assert.Equal(2, (await _service.GetAllAsync()).Count);
    }

    [Fact]
    public async Task AddRange_WritesNothingWhenAnyRowIsInvalid()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AddRangeAsync([Tx("2025-01-01", 100), Tx("2025-01-02", 0)]));

        Assert.Contains("zero", ex.Message);
        Assert.Empty(await _service.GetAllAsync());
    }

    [Fact]
    public async Task AddRange_GivesEveryRowAFreshId()
    {
        var row = Tx("2025-01-01", 100);
        row.Id = 999;

        await _service.AddRangeAsync([row, Tx("2025-01-02", 200)]);

        Assert.Equal(2, (await _service.GetAllAsync()).Select(t => t.Id).Distinct().Count());
        Assert.DoesNotContain(999, (await _service.GetAllAsync()).Select(t => t.Id));
    }

    [Fact]
    public async Task AddRange_EmptyInputDoesNothing()
    {
        Assert.Equal(0, await _service.AddRangeAsync([]));
        Assert.Empty(await _service.GetAllAsync());
    }
}
