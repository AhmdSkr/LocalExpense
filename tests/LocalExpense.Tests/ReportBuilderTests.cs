using LocalExpense.Services;

namespace LocalExpense.Tests;

public class ReportBuilderTests
{
    private static DateOnly D(int y, int m, int d) => new(y, m, d);

    private static CategoryDayTotals Day(string category, DateOnly date, long income, long expenses) =>
        new(category, date, new PeriodTotals(income, expenses));

    // Q1 2026 by month: Jan, Feb, Mar.
    private static readonly IReadOnlyList<Bucket> Quarter =
        PeriodBucketer.GetBuckets(new DateRange(D(2026, 1, 1), D(2026, 3, 31)), Granularity.Month);

    // ---- TotalsByBucket ----

    [Fact]
    public void TotalsByBucket_SumsIncomeAndExpensesPerBucketAndFillsEmptyBucketsWithZero()
    {
        var rows = new[]
        {
            Day("Salary", D(2026, 1, 1), 300_000, 0),
            Day("Food", D(2026, 1, 31), 0, 4_000),     // last day of January
            Day("Rent", D(2026, 1, 1), 0, 90_000),
            Day("Food", D(2026, 3, 1), 500, 1_000),    // first day of March; February stays empty
        };

        var totals = ReportBuilder.TotalsByBucket(Quarter, rows);

        Assert.Equal(Quarter, totals.Select(t => t.Bucket));
        Assert.Equal(
            new[] { new PeriodTotals(300_000, 94_000), default, new PeriodTotals(500, 1_000) },
            totals.Select(t => t.Totals));
    }

    [Fact]
    public void TotalsByBucket_IgnoresRowsOutsideEveryBucket()
    {
        var rows = new[] { Day("Food", D(2025, 12, 31), 0, 999), Day("Food", D(2026, 4, 1), 0, 999), Day("Food", D(2026, 2, 10), 0, 1) };

        Assert.Equal(
            new[] { default, new PeriodTotals(0, 1), default },
            ReportBuilder.TotalsByBucket(Quarter, rows).Select(t => t.Totals));
    }

    [Fact]
    public void TotalsByBucket_OfNoRowsIsAllZeroes() =>
        Assert.All(ReportBuilder.TotalsByBucket(Quarter, []), t => Assert.False(t.Totals.HasData));

    [Fact]
    public void TotalsByBucket_MatchesEachDayToItsClippedBucketInTheLastMonthWindow()
    {
        var window = new DateRange(D(2026, 9, 10), D(2026, 10, 9));
        var days = PeriodBucketer.GetBuckets(window, Granularity.Day);
        var rows = new[] { Day("Food", D(2026, 9, 10), 0, 1), Day("Food", D(2026, 9, 30), 0, 2), Day("Food", D(2026, 10, 1), 0, 3), Day("Food", D(2026, 10, 9), 0, 4) };

        var totals = ReportBuilder.TotalsByBucket(days, rows);

        Assert.Equal(30, totals.Count);
        Assert.Equal(new long[] { 1, 2, 3, 4 }, totals.Where(t => t.Totals.HasData).Select(t => t.Totals.ExpensesMinor));
        Assert.Equal(9, totals.Count(t => t.Bucket.Key.Month == 10));   // 21 September days, then 9 October days
    }

    [Fact]
    public void TotalsByBucket_PutsTheYearlyBucketsOfALongTimelineTogether()
    {
        var years = PeriodBucketer.GetBuckets(new DateRange(D(2023, 10, 1), D(2026, 9, 30)), Granularity.Year);
        var rows = new[] { Day("A", D(2023, 12, 31), 10, 0), Day("A", D(2024, 1, 1), 20, 0), Day("B", D(2026, 9, 30), 0, 5) };

        Assert.Equal(
            new[] { new PeriodTotals(10, 0), new PeriodTotals(20, 0), default, new PeriodTotals(0, 5) },
            ReportBuilder.TotalsByBucket(years, rows).Select(t => t.Totals));
    }

    // ---- ByCategory ----

    [Fact]
    public void ByCategory_ReportsTheNetOfEachCategoryInEachBucket()
    {
        var rows = new[]
        {
            Day("Food", D(2026, 1, 5), 0, 1_000),
            Day("Food", D(2026, 1, 20), 0, 500),
            Day("Food", D(2026, 3, 2), 200, 0),     // a refund: positive net
            Day("Salary", D(2026, 1, 1), 300_000, 0),
        };

        var table = ReportBuilder.ByCategory(Quarter, rows);

        var food = table.Rows.Single(r => r.Category == "Food");
        Assert.Equal(new long[] { -1_500, 0, 200 }, food.NetMinor);
        Assert.Equal(-1_300, food.TotalMinor);
        var salary = table.Rows.Single(r => r.Category == "Salary");
        Assert.Equal(new long[] { 300_000, 0, 0 }, salary.NetMinor);
    }

    [Fact]
    public void ByCategory_NetsIncomeAgainstExpensesWithinOneCategoryAndDay()
    {
        var table = ReportBuilder.ByCategory(Quarter, [Day("Salary", D(2026, 2, 1), 100_000, 2_000)]);

        Assert.Equal(new long[] { 0, 98_000, 0 }, table.Rows.Single().NetMinor);
    }

    [Fact]
    public void ByCategory_TotalsAddUpAcrossRowsColumnsAndTheGrandTotal()
    {
        var rows = new[]
        {
            Day("Food", D(2026, 1, 5), 0, 1_000), Day("Rent", D(2026, 1, 1), 0, 90_000), Day("Salary", D(2026, 1, 1), 300_000, 0),
            Day("Food", D(2026, 2, 5), 0, 2_000), Day("Salary", D(2026, 3, 1), 300_000, 0), Day("Gifts", D(2026, 3, 15), 750, 0),
        };

        var table = ReportBuilder.ByCategory(Quarter, rows);

        Assert.Equal(new long[] { 209_000, -2_000, 300_750 }, table.BucketNetMinor);
        Assert.Equal(table.Rows.Sum(r => r.TotalMinor), table.TotalMinor);
        Assert.Equal(table.BucketNetMinor.Sum(), table.TotalMinor);
        Assert.Equal(rows.Sum(r => r.Totals.NetMinor), table.TotalMinor);
        for (var i = 0; i < Quarter.Count; i++)
            Assert.Equal(table.Rows.Sum(r => r.NetMinor[i]), table.BucketNetMinor[i]);
    }

    [Fact]
    public void ByCategory_SortsByNameIgnoringCaseAndKeepsCaseVariantsAsSeparateRows()
    {
        var rows = new[]
        {
            Day("rent", D(2026, 1, 1), 0, 1), Day("Rent", D(2026, 1, 1), 0, 2), Day("Zoo", D(2026, 1, 1), 0, 3),
            Day("apples", D(2026, 1, 1), 0, 4), Day("Bills", D(2026, 1, 1), 0, 5),
        };

        Assert.Equal(new[] { "apples", "Bills", "Rent", "rent", "Zoo" }, ReportBuilder.ByCategory(Quarter, rows).Rows.Select(r => r.Category));
    }

    [Fact]
    public void ByCategory_OfNoRowsHasNoRowsAndZeroTotals()
    {
        var table = ReportBuilder.ByCategory(Quarter, []);

        Assert.Empty(table.Rows);
        Assert.Equal(new long[] { 0, 0, 0 }, table.BucketNetMinor);
        Assert.Equal(0, table.TotalMinor);
    }

    [Fact]
    public void ByCategory_KeepsACategoryWhoseNetIsZero()
    {
        var table = ReportBuilder.ByCategory(Quarter, [Day("Swap", D(2026, 1, 1), 500, 500)]);

        Assert.Equal(0, table.Rows.Single().TotalMinor);
    }
}
