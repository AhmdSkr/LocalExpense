using LocalExpense.Models;
using LocalExpense.Services;
using Microsoft.EntityFrameworkCore;

namespace LocalExpense.Tests;

public class TransactionServiceValidationTests : IDisposable
{
    private readonly TestDbFactory _factory = new();
    private readonly TransactionService _service;

    public TransactionServiceValidationTests() => _service = new TransactionService(_factory);

    public void Dispose() => _factory.Dispose();

    private static Transaction Valid() => new()
    {
        Date = new DateOnly(2026, 10, 9),
        AmountMinor = -1250,
        Category = "Food",
        Note = "Lunch",
    };

    private int RowCount()
    {
        using var db = _factory.CreateDbContext();
        return db.Transactions.Count();
    }

    [Fact]
    public async Task Add_RejectsZeroAmount()
    {
        var t = Valid();
        t.AmountMinor = 0;

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(t));
        Assert.Equal(0, RowCount());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Add_RejectsMissingCategory(string? category)
    {
        var t = Valid();
        t.Category = category!;

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(t));
        Assert.Equal(0, RowCount());
    }

    [Fact]
    public async Task Add_RejectsCategoryOver100Characters()
    {
        var t = Valid();
        t.Category = new string('c', 101);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(t));
        Assert.Equal(0, RowCount());
    }

    [Fact]
    public async Task Add_AcceptsCategoryOfExactly100Characters()
    {
        var t = Valid();
        t.Category = new string('c', 100);

        await _service.AddAsync(t);

        Assert.Equal(1, RowCount());
    }

    [Fact]
    public async Task Add_RejectsNoteOver500Characters()
    {
        var t = Valid();
        t.Note = new string('n', 501);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(t));
        Assert.Equal(0, RowCount());
    }

    [Fact]
    public async Task Add_AcceptsNoteOfExactly500Characters()
    {
        var t = Valid();
        t.Note = new string('n', 500);

        await _service.AddAsync(t);

        Assert.Equal(1, RowCount());
    }

    [Fact]
    public async Task Add_AcceptsNullNote()
    {
        var t = Valid();
        t.Note = null;

        var saved = await _service.AddAsync(t);

        using var db = _factory.CreateDbContext();
        Assert.Null(await db.Transactions.Where(x => x.Id == saved.Id).Select(x => x.Note).SingleAsync());
    }

    [Fact]
    public async Task Add_TrimsCategoryBeforeSaving()
    {
        var t = Valid();
        t.Category = "  Food ";

        var saved = await _service.AddAsync(t);

        using var db = _factory.CreateDbContext();
        Assert.Equal("Food", await db.Transactions.Where(x => x.Id == saved.Id).Select(x => x.Category).SingleAsync());
    }

    [Fact]
    public async Task Add_MeasuresCategoryLengthAfterTrimming()
    {
        var t = Valid();
        t.Category = " " + new string('c', 100) + " ";   // 102 characters raw, 100 once trimmed

        await _service.AddAsync(t);

        Assert.Equal(1, RowCount());
    }

    // The same normalization as Transaction.Create and the CSV importer, so no write path stores "   " or " lunch ".
    [Theory]
    [InlineData("   ", null)]
    [InlineData("  lunch  ", "lunch")]
    public async Task Add_TrimsTheNoteAndStoresABlankOneAsNull(string note, string? expected)
    {
        var t = Valid();
        t.Note = note;

        var saved = await _service.AddAsync(t);

        using var db = _factory.CreateDbContext();
        Assert.Equal(expected, await db.Transactions.Where(x => x.Id == saved.Id).Select(x => x.Note).SingleAsync());
    }

    [Theory]
    [InlineData(TransactionService.MaxAmountMinor)]
    [InlineData(-TransactionService.MaxAmountMinor)]
    public async Task Add_AcceptsAnAmountAtTheLimit(long amount)
    {
        var t = Valid();
        t.AmountMinor = amount;

        await _service.AddAsync(t);

        Assert.Equal(1, RowCount());
    }

    // The edit dialog cannot show more than this, so nothing beyond it may be stored.
    [Theory]
    [InlineData(TransactionService.MaxAmountMinor + 1)]
    [InlineData(-TransactionService.MaxAmountMinor - 1)]
    [InlineData(long.MinValue)]
    public async Task Add_RejectsAnAmountBeyondTheLimit(long amount)
    {
        var t = Valid();
        t.AmountMinor = amount;

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(t));

        Assert.Contains("$999,999,999.99", ex.Message);
        Assert.Equal(0, RowCount());
    }

    // Update runs the same validation; spot-check that it is wired in and leaves the row alone.
    [Fact]
    public async Task Update_AppliesTheSameValidationAndLeavesTheRowUnchanged()
    {
        var saved = await _service.AddAsync(Valid());
        var edited = new Transaction
        {
            Id = saved.Id,
            Date = saved.Date,
            AmountMinor = 0,
            Category = "Changed",
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(edited));

        using var db = _factory.CreateDbContext();
        var stored = await db.Transactions.SingleAsync();
        Assert.Equal(-1250, stored.AmountMinor);
        Assert.Equal("Food", stored.Category);
    }
}
