using LocalExpense.Models;

namespace LocalExpense.Services;

/// <summary>Prepares the bundled sample transactions for the demo window.</summary>
public static class SampleData
{
    /// <summary>
    /// Moves every date by the same whole number of months so the newest transaction falls in the month before
    /// <paramref name="today"/>, which keeps "Last month" and "Last year" in Reports full however old the sample is.
    /// Month-end days are clamped by <see cref="DateOnly.AddMonths"/> (31 March becomes 30 April). Changes the rows in place.
    /// </summary>
    public static void ShiftToEndLastMonth(IReadOnlyCollection<Transaction> rows, DateOnly today)
    {
        if (rows.Count == 0)
        {
            return;
        }

        var newest = rows.Max(t => t.Date);
        var months = MonthIndex(today) - 1 - MonthIndex(newest);
        foreach (var row in rows)
        {
            row.Date = row.Date.AddMonths(months);
        }
    }

    private static int MonthIndex(DateOnly date) => date.Year * 12 + date.Month - 1;
}
