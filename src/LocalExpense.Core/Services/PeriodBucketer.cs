using System.Globalization;

namespace LocalExpense.Services;

/// <summary>
/// Pure date arithmetic for the Reports form: which dates a <see cref="Period"/> covers, how finely to slice it,
/// and the (zero-filled) slices. Nothing here touches the database or the clock; "today" is always passed in.
/// </summary>
public static class PeriodBucketer
{
    /// <summary>The whole timeline switches from monthly to yearly buckets above this many calendar months.</summary>
    public const int MaxMonthlySpan = 24;

    /// <summary>
    /// The dates <paramref name="period"/> covers. <paramref name="bounds"/> (earliest and latest transaction) is only used by
    /// <see cref="PeriodKind.Timeline"/>, which has no range when it is null (no transactions).
    /// </summary>
    public static DateRange? GetRange(Period period, DateOnly today, DateRange? bounds) => period.Kind switch
    {
        PeriodKind.Timeline => bounds,
        PeriodKind.Year => new DateRange(new DateOnly(period.Year, 1, 1), new DateOnly(period.Year, 12, 31)),
        PeriodKind.Month => MonthRange(period.Year, period.Month),
        PeriodKind.LastMonth => new DateRange(today.AddMonths(-1).AddDays(1), today),
        PeriodKind.LastTwelveMonths =>new DateRange(today.AddYears(-1).AddDays(1), today),
        _ => throw new ArgumentOutOfRangeException(nameof(period)),
    };

    /// <summary>The years from the earliest to the latest transaction, ascending, including years without transactions.</summary>
    public static IReadOnlyList<int> GetYears(DateRange bounds) =>
        Enumerable.Range(bounds.From.Year, bounds.To.Year - bounds.From.Year + 1).ToList();

    /// <summary>Inclusive count of calendar months touched by the range.</summary>
    public static int MonthSpan(DateRange range) =>
        (range.To.Year - range.From.Year) * 12 + range.To.Month - range.From.Month + 1;

    /// <summary>
    /// A year or the last twelve months is sliced by month, and a single month or the last month by day. The whole timeline is sliced by month,
    /// or by year when it spans more than <see cref="MaxMonthlySpan"/> months and <paramref name="allowYearly"/> is set
    /// (the line chart passes false and always stays monthly).
    /// </summary>
    public static Granularity GetGranularity(Period period, DateRange range, bool allowYearly) => period.Kind switch
    {
        PeriodKind.Month or PeriodKind.LastMonth => Granularity.Day,
        PeriodKind.Timeline when allowYearly && MonthSpan(range) > MaxMonthlySpan => Granularity.Year,
        _ => Granularity.Month,
    };

    /// <summary>One bucket per day, month or year touching the range, in ascending order, clipped to the range.</summary>
    public static IReadOnlyList<Bucket> GetBuckets(DateRange range, Granularity granularity)
    {
        var buckets = new List<Bucket>();
        var key = FirstDayOf(range.From, granularity);
        while (key <= range.To)
        {
            var next = Next(key, granularity);
            buckets.Add(new Bucket(DateOnly.FromDayNumber(Math.Max(key.DayNumber, range.From.DayNumber)),
                                   DateOnly.FromDayNumber(Math.Min(next.DayNumber - 1, range.To.DayNumber)),
                                   key));
            key = next;
        }

        return buckets;
    }

    /// <summary>A column or axis label for a bucket, from its calendar key: 09-10 (day), 2026-09 (month) or 2026 (year). Invariant, so it never depends on the culture's calendar.</summary>
    public static string Label(Bucket bucket, Granularity granularity) => bucket.Key.ToString(granularity switch
    {
        Granularity.Day => "MM-dd",
        Granularity.Month => "yyyy-MM",
        _ => "yyyy",
    }, CultureInfo.InvariantCulture);

    private static DateRange MonthRange(int year, int month) =>
        new(new DateOnly(year, month, 1), new DateOnly(year, month, DateTime.DaysInMonth(year, month)));

    private static DateOnly FirstDayOf(DateOnly date, Granularity granularity) => granularity switch
    {
        Granularity.Day => date,
        Granularity.Month => new DateOnly(date.Year, date.Month, 1),
        Granularity.Year => new DateOnly(date.Year, 1, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(granularity)),
    };

    private static DateOnly Next(DateOnly key, Granularity granularity) => granularity switch
    {
        Granularity.Day => key.AddDays(1),
        Granularity.Month => key.AddMonths(1),
        _ => key.AddYears(1),
    };
}
