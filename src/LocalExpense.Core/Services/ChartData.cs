namespace LocalExpense.Services;

/// <summary>
/// What the Reports charts plot: one entry per bucket, in minor units (converted to decimals only when drawn), with the axis label
/// of each bucket. Net is income minus expenses per bucket, so a chart and the table beside it always add up.
/// </summary>
public sealed record ChartData(
    IReadOnlyList<string> Labels,
    IReadOnlyList<long> IncomeMinor,
    IReadOnlyList<long> ExpensesMinor,
    IReadOnlyList<long> NetMinor)
{
    public int Count => Labels.Count;

    public static ChartData From(IReadOnlyList<BucketTotals> buckets, Granularity granularity) => new(
        buckets.Select(b => PeriodBucketer.Label(b.Bucket, granularity)).ToList(),
        buckets.Select(b => b.Totals.IncomeMinor).ToList(),
        buckets.Select(b => b.Totals.ExpensesMinor).ToList(),
        buckets.Select(b => b.Totals.NetMinor).ToList());

    /// <summary>
    /// Show every Nth axis label so that no more than <paramref name="maxLabels"/> are drawn: 1 when they all fit,
    /// larger when the axis is crowded (the daily view of a month).
    /// </summary>
    public static int LabelStep(int labelCount, int maxLabels) =>
        labelCount <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling((double)labelCount / Math.Max(1, maxLabels)));
}
