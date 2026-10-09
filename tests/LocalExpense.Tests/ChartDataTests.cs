using LocalExpense.Services;

namespace LocalExpense.Tests;

public class ChartDataTests
{
    private static DateOnly D(int y, int m, int d) => new(y, m, d);

    private static readonly IReadOnlyList<Bucket> Quarter =
        PeriodBucketer.GetBuckets(new DateRange(D(2026, 1, 1), D(2026, 3, 31)), Granularity.Month);

    private static CategoryDayTotals Day(DateOnly date, long income, long expenses) => new("X", date, new PeriodTotals(income, expenses));

    [Fact]
    public void From_KeepsMinorUnitsAndComputesNetPerBucket()
    {
        var totals = ReportBuilder.TotalsByBucket(Quarter, [Day(D(2026, 1, 5), 300_000, 90_000), Day(D(2026, 3, 9), 100, 250)]);

        var data = ChartData.From(totals, Granularity.Month);

        Assert.Equal(new long[] { 300_000, 0, 100 }, data.IncomeMinor);
        Assert.Equal(new long[] { 90_000, 0, 250 }, data.ExpensesMinor);
        Assert.Equal(new long[] { 210_000, 0, -150 }, data.NetMinor);
        Assert.Equal(3, data.Count);
    }

    [Fact]
    public void From_LabelsEveryBucketIncludingEmptyOnes()
    {
        var data = ChartData.From(ReportBuilder.TotalsByBucket(Quarter, []), Granularity.Month);

        Assert.Equal(new[] { "2026-01", "2026-02", "2026-03" }, data.Labels);
        Assert.All(data.IncomeMinor.Concat(data.ExpensesMinor).Concat(data.NetMinor), v => Assert.Equal(0, v));
    }

    [Fact]
    public void From_NetEqualsIncomeMinusExpensesInEveryBucketAndSumsToThePeriodNet()
    {
        var rows = new[] { Day(D(2026, 1, 1), 10, 3), Day(D(2026, 2, 1), 0, 8), Day(D(2026, 3, 31), 5, 5) };

        var data = ChartData.From(ReportBuilder.TotalsByBucket(Quarter, rows), Granularity.Month);

        for (var i = 0; i < data.Count; i++)
            Assert.Equal(data.IncomeMinor[i] - data.ExpensesMinor[i], data.NetMinor[i]);
        Assert.Equal(rows.Sum(r => r.Totals.NetMinor), data.NetMinor.Sum());
    }

    [Theory]
    [InlineData(12, 12, 1)]    // all fit
    [InlineData(13, 12, 2)]    // one too many: every other label
    [InlineData(30, 15, 2)]
    [InlineData(31, 10, 4)]    // a 31-day month on a narrow axis
    [InlineData(5, 0, 5)]      // a zero or negative width never divides by zero
    [InlineData(0, 10, 1)]
    public void LabelStep_ShowsAtMostMaxLabels(int count, int max, int expected)
    {
        var step = ChartData.LabelStep(count, max);

        Assert.Equal(expected, step);
        if (max > 0 && count > 0)
            Assert.True((count + step - 1) / step <= max);   // labels at indexes 0, step, 2*step, ...
    }
}
