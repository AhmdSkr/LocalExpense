using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class SampleDataTests
{
    private static Transaction On(int y, int m, int d) => new() { Date = new DateOnly(y, m, d), AmountMinor = -1, Category = "X" };

    [Fact]
    public void ShiftToEndLastMonth_MovesTheNewestDateIntoThePreviousMonthAndEveryDateByTheSameMonths()
    {
        var rows = new[] { On(2023, 10, 1), On(2025, 6, 15), On(2026, 9, 20) };

        SampleData.ShiftToEndLastMonth(rows, today: new DateOnly(2027, 3, 15));   // newest must land in February 2027: +5 months

        Assert.Equal(new[] { new DateOnly(2024, 3, 1), new DateOnly(2025, 11, 15), new DateOnly(2027, 2, 20) }, rows.Select(t => t.Date));
    }

    [Fact]
    public void ShiftToEndLastMonth_MovesBackwardsWhenTheSampleEndsAfterLastMonth()
    {
        var rows = new[] { On(2026, 1, 10), On(2026, 9, 20) };

        SampleData.ShiftToEndLastMonth(rows, today: new DateOnly(2026, 5, 2));   // newest must land in April 2026: -5 months

        Assert.Equal(new[] { new DateOnly(2025, 8, 10), new DateOnly(2026, 4, 20) }, rows.Select(t => t.Date));
    }

    [Fact]
    public void ShiftToEndLastMonth_LeavesDatesAloneWhenTheSampleAlreadyEndsLastMonth()
    {
        var rows = new[] { On(2023, 10, 1), On(2026, 9, 30) };

        SampleData.ShiftToEndLastMonth(rows, today: new DateOnly(2026, 10, 9));

        Assert.Equal(new[] { new DateOnly(2023, 10, 1), new DateOnly(2026, 9, 30) }, rows.Select(t => t.Date));
    }

    [Fact]
    public void ShiftToEndLastMonth_ClampsMonthEndDays()
    {
        var rows = new[] { On(2026, 1, 31), On(2026, 3, 31) };

        SampleData.ShiftToEndLastMonth(rows, today: new DateOnly(2026, 5, 10));   // +1 month

        Assert.Equal(new[] { new DateOnly(2026, 2, 28), new DateOnly(2026, 4, 30) }, rows.Select(t => t.Date));
    }

    [Fact]
    public void ShiftToEndLastMonth_AcceptsNoRows() =>
        SampleData.ShiftToEndLastMonth([], new DateOnly(2026, 10, 9));

    // The same file the app embeds for its demo window (see LocalExpense.App.csproj), so a broken sample fails here, not on someone's screen.
    [Fact]
    public void TheBundledSampleCsvImportsWithoutErrors()
    {
        using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "samples", "lebanon-family-expenses.csv"));

        var result = CsvImporter.Read(reader);

        Assert.Empty(result.Errors);
        Assert.Equal(1383, result.Rows.Count);
    }
}
