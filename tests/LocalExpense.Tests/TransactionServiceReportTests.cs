using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class TransactionServiceReportTests : IDisposable
{
    private readonly TestDbFactory _factory = new();
    private readonly TransactionService _service;

    public TransactionServiceReportTests() => _service = new TransactionService(_factory);

    public void Dispose() => _factory.Dispose();

    private Task<Transaction> Add(DateOnly date, long amount, string category = "Food") =>
        _service.AddAsync(new Transaction { Date = date, AmountMinor = amount, Category = category });

    private static readonly DateOnly Oct1 = new(2026, 10, 1);
    private static readonly DateOnly Oct31 = new(2026, 10, 31);

    private static CategoryDayTotals Day(string category, DateOnly date, long income, long expenses) =>
        new(category, date, new PeriodTotals(income, expenses));

    private async Task<List<CategoryDayTotals>> Query(DateOnly from, DateOnly to) =>
        (await _service.GetDailyTotalsByCategoryAsync(from, to))
            .OrderBy(r => r.Date).ThenBy(r => r.Category, StringComparer.Ordinal).ToList();

    [Fact]
    public async Task GetDailyTotalsByCategory_GroupsByCategoryAndDay()
    {
        await Add(Oct1, -2_500, "Food");
        await Add(Oct1, -1_000, "Food");
        await Add(Oct1, -700, "Rent");
        await Add(new DateOnly(2026, 10, 5), -300, "Food");

        Assert.Equal(
            new[] { Day("Food", Oct1, 0, 3_500), Day("Rent", Oct1, 0, 700), Day("Food", new DateOnly(2026, 10, 5), 0, 300) },
            await Query(Oct1, Oct31));
    }

    [Fact]
    public async Task GetDailyTotalsByCategory_KeepsIncomeAndExpensesApartWithinOneCategoryAndDay()
    {
        await Add(Oct1, 100_000, "Salary");
        await Add(Oct1, -2_000, "Salary");   // a deduction booked in the same category the same day
        await Add(Oct1, 50, "Salary");

        Assert.Equal(new[] { Day("Salary", Oct1, 100_050, 2_000) }, await Query(Oct1, Oct31));
    }

    [Fact]
    public async Task GetDailyTotalsByCategory_IncludesBothBoundaryDatesAndExcludesTheDaysOutside()
    {
        await Add(Oct1.AddDays(-1), 1_000_000);
        await Add(Oct1, 1);
        await Add(Oct31, -2);
        await Add(Oct31.AddDays(1), -1_000_000);

        Assert.Equal(new[] { Day("Food", Oct1, 1, 0), Day("Food", Oct31, 0, 2) }, await Query(Oct1, Oct31));
    }

    [Fact]
    public async Task GetDailyTotalsByCategory_IsEmptyForAPeriodWithNoTransactions()
    {
        await Add(new DateOnly(2026, 9, 30), -500);

        Assert.Empty(await _service.GetDailyTotalsByCategoryAsync(Oct1, Oct31));
    }

    [Fact]
    public async Task GetDailyTotalsByCategory_KeepsCategoriesThatDifferOnlyByCaseApart()
    {
        await Add(Oct1, -100, "Food");
        await Add(Oct1, -200, "food");

        Assert.Equal(new[] { Day("Food", Oct1, 0, 100), Day("food", Oct1, 0, 200) }, await Query(Oct1, Oct31));
    }

    [Fact]
    public async Task GetDailyTotalsByCategory_KeepsLargeSumsInMinorUnitsWithoutLosingPrecision()
    {
        // A long, not an int or a double. Far beyond what AddAsync accepts for one row, so the rows go straight into the table:
        // what is under test is that the query sums in 64-bit integers.
        const long big = 4_000_000_000_000_000_000;
        await using (var db = _factory.CreateDbContext())
        {
            db.Transactions.AddRange(
                new Transaction { Date = Oct1, AmountMinor = big, Category = "Food" },
                new Transaction { Date = Oct1, AmountMinor = 1, Category = "Food" });
            await db.SaveChangesAsync();
        }

        Assert.Equal(big + 1, (await _service.GetDailyTotalsByCategoryAsync(Oct1, Oct31)).Single().Totals.IncomeMinor);
    }

    [Fact]
    public async Task GetDateBounds_IsNullWithNoTransactions() =>
        Assert.Null(await _service.GetDateBoundsAsync());

    [Fact]
    public async Task GetDateBounds_ReturnsTheEarliestAndLatestDates()
    {
        await Add(new DateOnly(2025, 6, 15), -1);
        await Add(new DateOnly(2023, 10, 1), -1);
        await Add(new DateOnly(2026, 9, 30), 1);
        await Add(new DateOnly(2024, 1, 1), 1);

        Assert.Equal(new DateRange(new DateOnly(2023, 10, 1), new DateOnly(2026, 9, 30)), await _service.GetDateBoundsAsync());
    }

    [Fact]
    public async Task GetDateBounds_OfASingleTransactionIsThatDayOnBothEnds()
    {
        await Add(Oct1, -1);

        Assert.Equal(new DateRange(Oct1, Oct1), await _service.GetDateBoundsAsync());
    }
}
