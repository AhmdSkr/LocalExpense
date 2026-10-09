using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class TransactionServiceQueryTests : IDisposable
{
    private readonly TestDbFactory _factory = new();
    private readonly TransactionService _service;

    public TransactionServiceQueryTests() => _service = new TransactionService(_factory);

    public void Dispose() => _factory.Dispose();

    private Task<Transaction> Add(DateOnly date, long amount, string category = "Food") =>
        _service.AddAsync(new Transaction { Date = date, AmountMinor = amount, Category = category });

    private static readonly DateOnly Oct1 = new(2026, 10, 1);
    private static readonly DateOnly Oct31 = new(2026, 10, 31);

    [Fact]
    public async Task GetByDateRange_IncludesBothBoundaryDatesAndExcludesTheDaysOutside()
    {
        await Add(new DateOnly(2026, 9, 30), -100);
        var first = await Add(Oct1, -200);
        var middle = await Add(new DateOnly(2026, 10, 15), -300);
        var last = await Add(Oct31, -400);
        await Add(new DateOnly(2026, 11, 1), -500);

        var ids = (await _service.GetByDateRangeAsync(Oct1, Oct31)).Select(t => t.Id).ToArray();

        Assert.Equal(new[] { last.Id, middle.Id, first.Id }, ids);   // newest first
    }

    [Fact]
    public async Task GetByDateRange_ReturnsOnlyThatDayWhenFromEqualsTo()
    {
        await Add(Oct1.AddDays(-1), -100);
        var target = await Add(Oct1, -200);
        await Add(Oct1.AddDays(1), -300);

        var result = await _service.GetByDateRangeAsync(Oct1, Oct1);

        Assert.Equal(target.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task GetByDateRange_ReturnsNothingWhenFromIsAfterTo()
    {
        await Add(new DateOnly(2026, 10, 15), -100);

        Assert.Empty(await _service.GetByDateRangeAsync(Oct31, Oct1));
    }

    [Fact]
    public async Task GetFiltered_WithNoBoundsReturnsEverythingNewestFirst()
    {
        var oldest = await Add(new DateOnly(2026, 1, 5), -100);
        var newest = await Add(new DateOnly(2026, 12, 5), -200);

        var ids = (await _service.GetFilteredAsync(null, null)).Select(t => t.Id).ToArray();

        Assert.Equal(new[] { newest.Id, oldest.Id }, ids);
    }

    [Fact]
    public async Task GetFiltered_WithOnlyFromKeepsThatDayAndLater()
    {
        await Add(Oct1.AddDays(-1), -100);
        var onDay = await Add(Oct1, -200);
        var later = await Add(Oct31, -300);

        var ids = (await _service.GetFilteredAsync(Oct1, null)).Select(t => t.Id).ToArray();

        Assert.Equal(new[] { later.Id, onDay.Id }, ids);
    }

    [Fact]
    public async Task GetFiltered_WithOnlyToKeepsThatDayAndEarlier()
    {
        var earlier = await Add(Oct1, -100);
        var onDay = await Add(Oct31, -200);
        await Add(Oct31.AddDays(1), -300);

        var ids = (await _service.GetFilteredAsync(null, Oct31)).Select(t => t.Id).ToArray();

        Assert.Equal(new[] { onDay.Id, earlier.Id }, ids);
    }

    [Fact]
    public async Task GetFiltered_WithBothBoundsMatchesGetByDateRange()
    {
        await Add(Oct1.AddDays(-1), -100);
        await Add(Oct1, -200);
        await Add(new DateOnly(2026, 10, 15), -300);
        await Add(Oct31, -400);
        await Add(Oct31.AddDays(1), -500);

        var filtered = (await _service.GetFilteredAsync(Oct1, Oct31)).Select(t => t.Id);
        var ranged = (await _service.GetByDateRangeAsync(Oct1, Oct31)).Select(t => t.Id);

        Assert.Equal(ranged, filtered);
        Assert.Equal(3, filtered.Count());
    }

    [Fact]
    public async Task GetFiltered_ReturnsNothingWhenFromIsAfterTo()
    {
        await Add(new DateOnly(2026, 10, 15), -100);

        Assert.Empty(await _service.GetFilteredAsync(Oct31, Oct1));
    }

    [Fact]
    public async Task GetFiltered_ReturnsNothingWhenNoRowFallsInTheRange()
    {
        await Add(new DateOnly(2026, 9, 1), -100);

        Assert.Empty(await _service.GetFilteredAsync(Oct1, Oct31));
    }

    [Fact]
    public async Task GetFiltered_ReturnsOnlyThatDayWhenFromEqualsTo()
    {
        await Add(Oct1.AddDays(-1), -100);
        var target = await Add(Oct1, -200);
        await Add(Oct1.AddDays(1), -300);

        var result = await _service.GetFilteredAsync(Oct1, Oct1);

        Assert.Equal(target.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task GetFiltered_OrdersByDateThenIdDescendingInsideTheRange()
    {
        var firstOnOct10 = await Add(new DateOnly(2026, 10, 10), -100);
        var oct20 = await Add(new DateOnly(2026, 10, 20), -200);
        var secondOnOct10 = await Add(new DateOnly(2026, 10, 10), -300);

        var ids = (await _service.GetFilteredAsync(Oct1, Oct31)).Select(t => t.Id).ToArray();

        Assert.Equal(new[] { oct20.Id, secondOnOct10.Id, firstOnOct10.Id }, ids);
    }

    [Fact]
    public async Task GetFiltered_HandlesARangeThatCrossesAYearBoundary()
    {
        await Add(new DateOnly(2025, 12, 30), -100);
        var dec31 = await Add(new DateOnly(2025, 12, 31), -200);
        var jan1 = await Add(new DateOnly(2026, 1, 1), -300);
        await Add(new DateOnly(2026, 1, 2), -400);

        var ids = (await _service.GetFilteredAsync(new DateOnly(2025, 12, 31), new DateOnly(2026, 1, 1)))
            .Select(t => t.Id).ToArray();

        Assert.Equal(new[] { jan1.Id, dec31.Id }, ids);
    }

    [Fact]
    public async Task GetFiltered_AcceptsTheExtremeDateOnlyValuesAsBounds()
    {
        await Add(new DateOnly(1, 1, 1), -100);
        await Add(new DateOnly(2026, 10, 15), -200);
        await Add(new DateOnly(9999, 12, 31), -300);

        var all = await _service.GetFilteredAsync(DateOnly.MinValue, DateOnly.MaxValue);

        Assert.Equal(3, all.Count);
    }

    [Fact]
    public async Task GetFiltered_ReturnsAllFieldsOfTheMatchingRows()
    {
        var saved = await _service.AddAsync(new Transaction
        {
            Date = new DateOnly(2026, 10, 15),
            AmountMinor = -4_525,
            Category = "Food",
            Note = "Groceries",
        });

        var row = Assert.Single(await _service.GetFilteredAsync(Oct1, Oct31));

        Assert.Equal(saved.Id, row.Id);
        Assert.Equal(new DateOnly(2026, 10, 15), row.Date);
        Assert.Equal(-4_525, row.AmountMinor);
        Assert.Equal("Food", row.Category);
        Assert.Equal("Groceries", row.Note);
    }

    [Fact]
    public async Task GetFiltered_NoLongerReturnsARowEditedOutOfTheRange()
    {
        var saved = await Add(new DateOnly(2026, 10, 15), -100);
        Assert.Single(await _service.GetFilteredAsync(Oct1, Oct31));

        await _service.UpdateAsync(new Transaction
        {
            Id = saved.Id,
            Date = new DateOnly(2026, 11, 15),
            AmountMinor = -100,
            Category = "Food",
        });

        Assert.Empty(await _service.GetFilteredAsync(Oct1, Oct31));
        Assert.Single(await _service.GetFilteredAsync(new DateOnly(2026, 11, 1), null));
    }

    [Fact]
    public async Task GetFiltered_NoLongerReturnsADeletedRow()
    {
        var kept = await Add(new DateOnly(2026, 10, 10), -100);
        var removed = await Add(new DateOnly(2026, 10, 20), -200);

        await _service.DeleteAsync(removed.Id);

        Assert.Equal(kept.Id, Assert.Single(await _service.GetFilteredAsync(Oct1, Oct31)).Id);
    }

    [Fact]
    public async Task GetFiltered_WithACategoryReturnsOnlyThatCategory()
    {
        var food = await Add(Oct1, -100, "Food");
        await Add(Oct1, -200, "Rent");
        var food2 = await Add(Oct31, -300, "Food");

        var ids = (await _service.GetFilteredAsync(null, null, "Food")).Select(t => t.Id).ToArray();

        Assert.Equal(new[] { food2.Id, food.Id }, ids);
    }

    [Fact]
    public async Task GetFiltered_CombinesTheCategoryWithTheDateRange()
    {
        await Add(Oct1.AddDays(-1), -100, "Food");
        var inRange = await Add(new DateOnly(2026, 10, 15), -200, "Food");
        await Add(new DateOnly(2026, 10, 15), -300, "Rent");
        await Add(Oct31.AddDays(1), -400, "Food");

        var result = await _service.GetFilteredAsync(Oct1, Oct31, "Food");

        Assert.Equal(inRange.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task GetFiltered_WithNoCategoryReturnsEveryCategory()
    {
        await Add(Oct1, -100, "Food");
        await Add(Oct1, -200, "Rent");

        Assert.Equal(2, (await _service.GetFilteredAsync(null, null, null)).Count);
    }

    [Fact]
    public async Task GetFiltered_ReturnsNothingForAnUnknownCategory()
    {
        await Add(Oct1, -100, "Food");

        Assert.Empty(await _service.GetFilteredAsync(null, null, "Travel"));
    }

    [Fact]
    public async Task GetFiltered_MatchesTheCategoryExactlyIncludingCase()
    {
        // GetCategoriesAsync lists "Food" and "food" as two categories, so the filter keeps them apart too.
        var upper = await Add(Oct1, -100, "Food");
        var lower = await Add(Oct1, -200, "food");

        Assert.Equal(upper.Id, Assert.Single(await _service.GetFilteredAsync(null, null, "Food")).Id);
        Assert.Equal(lower.Id, Assert.Single(await _service.GetFilteredAsync(null, null, "food")).Id);
    }

    [Fact]
    public async Task GetFiltered_ReturnsNothingWhenFromIsAfterToEvenForAMatchingCategory()
    {
        await Add(new DateOnly(2026, 10, 15), -100, "Food");

        Assert.Empty(await _service.GetFilteredAsync(Oct31, Oct1, "Food"));
    }

    [Fact]
    public async Task GetFiltered_StopsWhenTheTokenIsAlreadyCancelled()
    {
        await Add(new DateOnly(2026, 10, 15), -100);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.GetFilteredAsync(Oct1, Oct31, ct: cts.Token));
    }

    [Fact]
    public async Task GetTotal_IsTheNetOfIncomeMinusExpenses()
    {
        await Add(new DateOnly(2026, 10, 2), 500_000, "Salary");
        await Add(new DateOnly(2026, 10, 3), -120_050, "Rent");
        await Add(new DateOnly(2026, 10, 4), -4_525, "Food");

        Assert.Equal(375_425, await _service.GetTotalAsync(Oct1, Oct31));
    }

    [Fact]
    public async Task GetTotal_CountsOnlyRowsInsideTheRange()
    {
        await Add(new DateOnly(2026, 9, 30), 1_000);
        await Add(Oct1, 20);
        await Add(Oct31, 300);
        await Add(new DateOnly(2026, 11, 1), 4_000);

        Assert.Equal(320, await _service.GetTotalAsync(Oct1, Oct31));
    }

    [Fact]
    public async Task GetTotal_IsZeroForAnEmptyRange()
    {
        await Add(new DateOnly(2026, 9, 1), 1_000);

        Assert.Equal(0, await _service.GetTotalAsync(Oct1, Oct31));
    }

    [Fact]
    public async Task GetTotal_StaysCorrectBeyondTheIntRange()
    {
        await Add(new DateOnly(2026, 10, 2), 3_000_000_000);
        await Add(new DateOnly(2026, 10, 3), 2_000_000_000);

        Assert.Equal(5_000_000_000, await _service.GetTotalAsync(Oct1, Oct31));
    }

    [Fact]
    public async Task GetTotalsByCategory_SumsPerCategoryAndSkipsCategoriesOutsideTheRange()
    {
        await Add(new DateOnly(2026, 10, 2), -1_000, "Food");
        await Add(new DateOnly(2026, 10, 9), -2_500, "Food");
        await Add(new DateOnly(2026, 10, 5), 100_000, "Salary");
        await Add(new DateOnly(2026, 10, 6), -50_000, "Rent");
        await Add(new DateOnly(2026, 9, 6), -99_999, "Travel");   // outside the range

        var totals = await _service.GetTotalsByCategoryAsync(Oct1, Oct31);

        Assert.Equal(3, totals.Count);
        Assert.Equal(-3_500, totals["Food"]);
        Assert.Equal(100_000, totals["Salary"]);
        Assert.Equal(-50_000, totals["Rent"]);
        Assert.False(totals.ContainsKey("Travel"));
    }

    [Fact]
    public async Task GetTotalsByCategory_NetsIncomeAndExpensesWithinOneCategory()
    {
        await Add(new DateOnly(2026, 10, 2), -10_000, "Shopping");
        await Add(new DateOnly(2026, 10, 3), 2_500, "Shopping");   // a refund

        var totals = await _service.GetTotalsByCategoryAsync(Oct1, Oct31);

        Assert.Equal(-7_500, Assert.Single(totals).Value);
    }

    [Fact]
    public async Task GetTotalsByCategory_ReturnsAnEmptyDictionaryWhenNothingMatches() =>
        Assert.Empty(await _service.GetTotalsByCategoryAsync(Oct1, Oct31));

    // Pins current behaviour: SQLite compares text case-sensitively, so "Food" and "food" are two categories.
    // If you decide categories should be case-insensitive, this test is the one to change.
    [Fact]
    public async Task GetTotalsByCategory_TreatsCategoriesCaseSensitively()
    {
        await Add(new DateOnly(2026, 10, 2), -100, "Food");
        await Add(new DateOnly(2026, 10, 3), -200, "food");

        var totals = await _service.GetTotalsByCategoryAsync(Oct1, Oct31);

        Assert.Equal(2, totals.Count);
        Assert.Equal(-100, totals["Food"]);
        Assert.Equal(-200, totals["food"]);
    }

    [Fact]
    public async Task GetCategories_ReturnsDistinctNamesInSortedOrder()
    {
        await Add(Oct1, -1, "Rent");
        await Add(Oct1, -1, "Food");
        await Add(Oct1, -1, "Rent");
        await Add(Oct1, -1, "Fun");

        Assert.Equal(new[] { "Food", "Fun", "Rent" }, await _service.GetCategoriesAsync());
    }

    // Same order as the Reports table (ReportBuilder.ByCategory), not SQLite's byte order, which would put "Zoo" before "apples".
    [Fact]
    public async Task GetCategories_SortsIgnoringCaseAndKeepsCaseVariantsApart()
    {
        foreach (var category in new[] { "rent", "Rent", "Zoo", "apples", "Bills" })
            await Add(Oct1, -1, category);

        Assert.Equal(new[] { "apples", "Bills", "Rent", "rent", "Zoo" }, await _service.GetCategoriesAsync());
    }

    [Fact]
    public async Task GetCategories_ReturnsAnEmptyListWhenThereAreNoRows() =>
        Assert.Empty(await _service.GetCategoriesAsync());
}
