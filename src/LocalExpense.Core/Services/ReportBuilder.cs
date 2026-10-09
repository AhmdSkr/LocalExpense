namespace LocalExpense.Services;

/// <summary>Income and expenses of one category on one day, in minor units. The database query returns one of these per category and day.</summary>
public readonly record struct CategoryDayTotals(string Category, DateOnly Date, PeriodTotals Totals);

/// <summary>One table row: the net (income minus expenses) of a category in each bucket, and over the whole period.</summary>
public sealed record CategoryRow(string Category, IReadOnlyList<long> NetMinor, long TotalMinor);

/// <summary>
/// Categories by buckets. <see cref="BucketNetMinor"/> is the net of every category in each bucket, and
/// <see cref="TotalMinor"/> the net of the whole period; both are sums of the rows, so the table always adds up.
/// </summary>
public sealed record CategoryTable(
    IReadOnlyList<Bucket> Buckets,
    IReadOnlyList<CategoryRow> Rows,
    IReadOnlyList<long> BucketNetMinor,
    long TotalMinor);

/// <summary>Pure aggregation of per-category daily totals into the buckets of a period. Rows dated outside every bucket are ignored.</summary>
public static class ReportBuilder
{
    /// <summary>Income and expenses of every bucket, zero where nothing happened, in bucket order.</summary>
    public static IReadOnlyList<BucketTotals> TotalsByBucket(IReadOnlyList<Bucket> buckets, IEnumerable<CategoryDayTotals> rows)
    {
        var income = new long[buckets.Count];
        var expenses = new long[buckets.Count];
        foreach (var row in rows)
        {
            var index = IndexOf(buckets, row.Date);
            if (index < 0)
            {
                continue;
            }

            income[index] += row.Totals.IncomeMinor;
            expenses[index] += row.Totals.ExpensesMinor;
        }

        return buckets.Select((bucket, i) => new BucketTotals(bucket, new PeriodTotals(income[i], expenses[i]))).ToList();
    }

    /// <summary>
    /// One row per category that has data in the period, in <see cref="CategoryOrder"/>, each with a net amount for every bucket.
    /// Categories are matched exactly, as everywhere else, so "food" and "Food" are two rows.
    /// </summary>
    public static CategoryTable ByCategory(IReadOnlyList<Bucket> buckets, IEnumerable<CategoryDayTotals> rows)
    {
        var perCategory = new Dictionary<string, long[]>(StringComparer.Ordinal);
        var bucketNet = new long[buckets.Count];
        foreach (var row in rows)
        {
            var index = IndexOf(buckets, row.Date);
            if (index < 0)
            {
                continue;
            }

            if (!perCategory.TryGetValue(row.Category, out var net))
            {
                perCategory[row.Category] = net = new long[buckets.Count];
            }

            net[index] += row.Totals.NetMinor;
            bucketNet[index] += row.Totals.NetMinor;
        }

        var categoryRows = perCategory
            .OrderByCategory(kv => kv.Key)
            .Select(kv => new CategoryRow(kv.Key, kv.Value, kv.Value.Sum()))
            .ToList();
        return new CategoryTable(buckets, categoryRows, bucketNet, bucketNet.Sum());
    }

    // Buckets tile their period in ascending order, so a binary search finds the one containing the date.
    private static int IndexOf(IReadOnlyList<Bucket> buckets, DateOnly date)
    {
        int low = 0, high = buckets.Count - 1;
        while (low <= high)
        {
            var mid = (low + high) / 2;
            if (date < buckets[mid].From)
            {
                high = mid - 1;
            }
            else if (date > buckets[mid].To)
            {
                low = mid + 1;
            }
            else
            {
                return mid;
            }
        }

        return -1;
    }
}
