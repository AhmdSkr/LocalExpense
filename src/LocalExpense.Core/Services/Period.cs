namespace LocalExpense.Services;

public enum PeriodKind
{
    /// <summary>Earliest to latest transaction.</summary>
    Timeline,

    /// <summary>One calendar year.</summary>
    Year,

    /// <summary>One calendar month.</summary>
    Month,

    /// <summary>The month ending today: from the day after today's date a month ago, through today.</summary>
    LastMonth,

    /// <summary>The 12 months ending today: from the day after today's date a year ago, through today.</summary>
    LastTwelveMonths,
}

/// <summary>What the user picked in the Reports form. Independent of "today"; <see cref="PeriodBucketer"/> resolves it.</summary>
public readonly record struct Period(PeriodKind Kind, int Year = 0, int Month = 0)
{
    public static Period Timeline => new(PeriodKind.Timeline);

    public static Period LastTwelveMonths => new(PeriodKind.LastTwelveMonths);

    public static Period ForYear(int year) => new(PeriodKind.Year, year);

    public static Period ForMonth(int year, int month) => new(PeriodKind.Month, year, month);

    public static Period LastMonth => new(PeriodKind.LastMonth);
}

/// <summary>An inclusive date range.</summary>
public readonly record struct DateRange(DateOnly From, DateOnly To);

public enum Granularity
{
    Day,
    Month,
    Year,
}

/// <summary>
/// One slice of a period. <see cref="From"/> and <see cref="To"/> are inclusive and clipped to the period, so the buckets
/// of a period tile it exactly. <see cref="Key"/> is the first day of the calendar day, month or year the bucket belongs to
/// (unclipped), which is what labels are formatted from and what grouped query results are matched on.
/// </summary>
public readonly record struct Bucket(DateOnly From, DateOnly To, DateOnly Key);

/// <summary>Income and expenses in minor units. Expenses are a magnitude (never negative).</summary>
public readonly record struct PeriodTotals(long IncomeMinor, long ExpensesMinor)
{
    public long NetMinor => IncomeMinor - ExpensesMinor;

    /// <summary>
    /// False when nothing was recorded. Valid because stored amounts are never zero, so any transaction
    /// makes income or expenses non-zero.
    /// </summary>
    public bool HasData => IncomeMinor != 0 || ExpensesMinor != 0;
}

public readonly record struct BucketTotals(Bucket Bucket, PeriodTotals Totals);
