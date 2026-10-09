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

    [Fact]
    public async Task GetCategories_ReturnsAnEmptyListWhenThereAreNoRows() =>
        Assert.Empty(await _service.GetCategoriesAsync());
}
