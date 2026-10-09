using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class TransactionServiceCrudTests : IDisposable
{
    private readonly TestDbFactory _factory = new();
    private readonly TransactionService _service;

    public TransactionServiceCrudTests() => _service = new TransactionService(_factory);

    public void Dispose() => _factory.Dispose();

    private static Transaction New(DateOnly? date = null, long amount = -1250, string category = "Food", string? note = "Lunch") => new()
    {
        Date = date ?? new DateOnly(2026, 10, 9),
        AmountMinor = amount,
        Category = category,
        Note = note,
    };

    [Fact]
    public async Task Add_AssignsAnIdAndTheRowReadsBackWithTheSameValues()
    {
        var saved = await _service.AddAsync(New(new DateOnly(2026, 2, 28), 99_999, "Salary", "October pay"));

        Assert.True(saved.Id > 0);
        var stored = await _service.GetByIdAsync(saved.Id);
        Assert.NotNull(stored);
        Assert.Equal(new DateOnly(2026, 2, 28), stored.Date);
        Assert.Equal(99_999, stored.AmountMinor);
        Assert.Equal("Salary", stored.Category);
        Assert.Equal("October pay", stored.Note);
    }

    [Fact]
    public async Task Add_IgnoresAPresetId()
    {
        var t = New();
        t.Id = 999;

        var saved = await _service.AddAsync(t);

        Assert.NotEqual(999, saved.Id);
        Assert.Null(await _service.GetByIdAsync(999));
        Assert.NotNull(await _service.GetByIdAsync(saved.Id));
    }

    [Fact]
    public async Task GetById_ReturnsNullForAnUnknownId() =>
        Assert.Null(await _service.GetByIdAsync(12345));

    [Fact]
    public async Task Update_ChangesAllFourFieldsAndReturnsTrue()
    {
        var saved = await _service.AddAsync(New());

        var ok = await _service.UpdateAsync(new Transaction
        {
            Id = saved.Id,
            Date = new DateOnly(2027, 1, 31),
            AmountMinor = 5_000,
            Category = "Refund",
            Note = "Changed note",
        });

        Assert.True(ok);
        var stored = await _service.GetByIdAsync(saved.Id);
        Assert.NotNull(stored);
        Assert.Equal(new DateOnly(2027, 1, 31), stored.Date);
        Assert.Equal(5_000, stored.AmountMinor);
        Assert.Equal("Refund", stored.Category);
        Assert.Equal("Changed note", stored.Note);
    }

    [Fact]
    public async Task Update_CanClearTheNote()
    {
        var saved = await _service.AddAsync(New(note: "Lunch"));

        await _service.UpdateAsync(new Transaction
        {
            Id = saved.Id,
            Date = saved.Date,
            AmountMinor = saved.AmountMinor,
            Category = saved.Category,
            Note = null,
        });

        Assert.Null((await _service.GetByIdAsync(saved.Id))!.Note);
    }

    [Fact]
    public async Task Update_ReturnsFalseForAnUnknownIdAndChangesNothing()
    {
        var existing = await _service.AddAsync(New());

        var ok = await _service.UpdateAsync(new Transaction
        {
            Id = existing.Id + 100,
            Date = new DateOnly(2030, 1, 1),
            AmountMinor = 1,
            Category = "Other",
        });

        Assert.False(ok);
        var all = await _service.GetAllAsync();
        var only = Assert.Single(all);
        Assert.Equal("Food", only.Category);
        Assert.Equal(-1250, only.AmountMinor);
    }

    [Fact]
    public async Task Delete_RemovesOnlyTheTargetRowAndReturnsTrue()
    {
        var keep = await _service.AddAsync(New(category: "Keep"));
        var remove = await _service.AddAsync(New(category: "Remove"));

        var ok = await _service.DeleteAsync(remove.Id);

        Assert.True(ok);
        Assert.Null(await _service.GetByIdAsync(remove.Id));
        Assert.NotNull(await _service.GetByIdAsync(keep.Id));
    }

    [Fact]
    public async Task Delete_ReturnsFalseForAnUnknownIdAndKeepsExistingRows()
    {
        var existing = await _service.AddAsync(New());

        var ok = await _service.DeleteAsync(existing.Id + 100);

        Assert.False(ok);
        Assert.NotNull(await _service.GetByIdAsync(existing.Id));
    }

    [Fact]
    public async Task DeleteMany_RemovesOnlyTheListedRowsAndReturnsTheCount()
    {
        var a = await _service.AddAsync(New());
        var b = await _service.AddAsync(New());
        var keep = await _service.AddAsync(New());

        var deleted = await _service.DeleteManyAsync([a.Id, b.Id]);

        Assert.Equal(2, deleted);
        Assert.Null(await _service.GetByIdAsync(a.Id));
        Assert.Null(await _service.GetByIdAsync(b.Id));
        Assert.NotNull(await _service.GetByIdAsync(keep.Id));
    }

    [Fact]
    public async Task DeleteMany_IgnoresUnknownIdsAndCountsOnlyRealDeletes()
    {
        var existing = await _service.AddAsync(New());

        var deleted = await _service.DeleteManyAsync([existing.Id, existing.Id + 100]);

        Assert.Equal(1, deleted);
        Assert.Null(await _service.GetByIdAsync(existing.Id));
    }

    [Fact]
    public async Task DeleteMany_WithNoIdsDeletesNothing()
    {
        var existing = await _service.AddAsync(New());

        Assert.Equal(0, await _service.DeleteManyAsync([]));
        Assert.NotNull(await _service.GetByIdAsync(existing.Id));
    }

    [Fact]
    public async Task GetAll_ReturnsAnEmptyListWhenThereAreNoRows() =>
        Assert.Empty(await _service.GetAllAsync());

    [Fact]
    public async Task GetAll_OrdersByDateDescendingThenIdDescending()
    {
        var oldest = await _service.AddAsync(New(new DateOnly(2026, 1, 1)));
        var newestFirstOfDay = await _service.AddAsync(New(new DateOnly(2026, 3, 1)));
        var middle = await _service.AddAsync(New(new DateOnly(2026, 2, 1)));
        var newestSecondOfDay = await _service.AddAsync(New(new DateOnly(2026, 3, 1)));

        var ids = (await _service.GetAllAsync()).Select(t => t.Id).ToArray();

        // 2026-03-01 twice (higher Id first), then 2026-02-01, then 2026-01-01
        Assert.Equal(new[] { newestSecondOfDay.Id, newestFirstOfDay.Id, middle.Id, oldest.Id }, ids);
    }
}
