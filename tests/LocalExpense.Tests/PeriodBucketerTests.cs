using System.Globalization;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class PeriodBucketerTests
{
    private static readonly DateOnly Today = new(2026, 10, 9);

    private static DateOnly D(int y, int m, int d) => new(y, m, d);

    private static DateRange R(DateOnly from, DateOnly to) => new(from, to);

    // ---- GetRange / Period.LastMonth ----

    [Fact]
    public void GetRange_Year_IsJanuaryFirstThroughDecemberThirtyFirst() =>
        Assert.Equal(R(D(2025, 1, 1), D(2025, 12, 31)), PeriodBucketer.GetRange(Period.ForYear(2025), Today, null));

    [Theory]
    [InlineData(2024, 2, 29)]   // leap year
    [InlineData(2025, 2, 28)]
    [InlineData(2025, 4, 30)]
    [InlineData(2025, 12, 31)]
    public void GetRange_Month_EndsOnTheLastDayOfThatMonth(int year, int month, int lastDay) =>
        Assert.Equal(R(D(year, month, 1), D(year, month, lastDay)), PeriodBucketer.GetRange(Period.ForMonth(year, month), Today, null));

    [Fact]
    public void GetRange_Timeline_IsTheBoundsAndNullWhenThereAreNone()
    {
        var bounds = R(D(2023, 10, 1), D(2026, 9, 30));

        Assert.Equal(bounds, PeriodBucketer.GetRange(Period.Timeline, Today, bounds));
        Assert.Null(PeriodBucketer.GetRange(Period.Timeline, Today, null));
    }

    [Fact]
    public void GetRange_LastTwelveMonths_StartsTheDayAfterTodayOneYearAgo() =>
        Assert.Equal(R(D(2025, 10, 10), Today), PeriodBucketer.GetRange(Period.LastTwelveMonths, Today, null));

    [Fact]
    public void GetRange_LastTwelveMonths_OnALeapDayStartsOnMarchFirst() =>
        Assert.Equal(R(D(2023, 3, 1), D(2024, 2, 29)), PeriodBucketer.GetRange(Period.LastTwelveMonths, D(2024, 2, 29), null));

    [Theory]
    [InlineData(2026, 10, 9, 2026, 9, 10)]    // 2026-09-10 .. 2026-10-09
    [InlineData(2026, 1, 15, 2025, 12, 16)]   // crosses the year boundary
    [InlineData(2026, 3, 31, 2026, 3, 1)]     // Feb 28 + 1 day: the window is the 31 days of March, not Feb 28 .. Mar 31
    [InlineData(2026, 3, 1, 2026, 2, 2)]      // 28 days
    [InlineData(2024, 3, 1, 2024, 2, 2)]      // leap year: 29 days
    public void GetRange_LastMonth_StartsTheDayAfterTodayOneMonthAgo(int y, int m, int d, int fy, int fm, int fd) =>
        Assert.Equal(R(D(fy, fm, fd), D(y, m, d)), PeriodBucketer.GetRange(Period.LastMonth, D(y, m, d), null));

    // ---- GetYears / MonthSpan ----

    [Fact]
    public void GetYears_ListsEveryYearFromEarliestToLatestIncludingGaps() =>
        Assert.Equal(new[] { 2022, 2023, 2024, 2025 }, PeriodBucketer.GetYears(R(D(2022, 6, 1), D(2025, 1, 1))));

    [Theory]
    [InlineData(2026, 3, 1, 2026, 3, 31, 1)]
    [InlineData(2026, 12, 31, 2027, 1, 1, 2)]    // two calendar months even though two days apart
    [InlineData(2023, 1, 1, 2024, 12, 31, 24)]
    [InlineData(2023, 1, 1, 2025, 1, 1, 25)]
    public void MonthSpan_CountsInclusiveCalendarMonths(int fy, int fm, int fd, int ty, int tm, int td, int expected) =>
        Assert.Equal(expected, PeriodBucketer.MonthSpan(R(D(fy, fm, fd), D(ty, tm, td))));

    // ---- GetGranularity ----

    [Fact]
    public void GetGranularity_TimelineOfExactly24Months_StaysMonthly() =>
        Assert.Equal(Granularity.Month, PeriodBucketer.GetGranularity(Period.Timeline, R(D(2023, 1, 20), D(2024, 12, 3)), allowYearly: true));

    [Fact]
    public void GetGranularity_TimelineOf25Months_SwitchesToYearly() =>
        Assert.Equal(Granularity.Year, PeriodBucketer.GetGranularity(Period.Timeline, R(D(2023, 1, 20), D(2025, 1, 3)), allowYearly: true));

    [Fact]
    public void GetGranularity_TimelineOf25Months_StaysMonthlyWhenYearlyIsNotAllowed() =>
        Assert.Equal(Granularity.Month, PeriodBucketer.GetGranularity(Period.Timeline, R(D(2023, 1, 20), D(2025, 1, 3)), allowYearly: false));

    [Fact]
    public void GetGranularity_YearAndLastTwelveMonthsAreMonthly_MonthIsDaily()
    {
        Assert.Equal(Granularity.Month, PeriodBucketer.GetGranularity(Period.ForYear(2025), R(D(2025, 1, 1), D(2025, 12, 31)), allowYearly: true));
        Assert.Equal(Granularity.Month, PeriodBucketer.GetGranularity(Period.LastTwelveMonths, R(D(2025, 10, 10), Today), allowYearly: true));
        Assert.Equal(Granularity.Day, PeriodBucketer.GetGranularity(Period.ForMonth(2026, 9), R(D(2026, 9, 1), D(2026, 9, 30)), allowYearly: true));
        Assert.Equal(Granularity.Day, PeriodBucketer.GetGranularity(Period.LastMonth, R(D(2026, 9, 10), Today), allowYearly: true));
    }

    // ---- GetBuckets ----

    [Fact]
    public void GetBuckets_Year_IsTwelveMonthsStartingEachFirst()
    {
        var buckets = PeriodBucketer.GetBuckets(R(D(2025, 1, 1), D(2025, 12, 31)), Granularity.Month);

        Assert.Equal(Enumerable.Range(1, 12).Select(m => D(2025, m, 1)), buckets.Select(b => b.Key));
        Assert.Equal(new Bucket(D(2025, 2, 1), D(2025, 2, 28), D(2025, 2, 1)), buckets[1]);
    }

    [Fact]
    public void GetBuckets_LeapFebruary_HasTwentyNineDailyBuckets()
    {
        var buckets = PeriodBucketer.GetBuckets(R(D(2024, 2, 1), D(2024, 2, 29)), Granularity.Day);

        Assert.Equal(29, buckets.Count);
        Assert.All(buckets, b => Assert.Equal(b.From, b.To));
    }

    [Fact]
    public void GetBuckets_LastTwelveMonths_IsThirteenMonthsWithPartialFirstAndLast()
    {
        var buckets = PeriodBucketer.GetBuckets(R(D(2025, 10, 10), Today), Granularity.Month);

        Assert.Equal(13, buckets.Count);
        Assert.Equal(new Bucket(D(2025, 10, 10), D(2025, 10, 31), D(2025, 10, 1)), buckets[0]);
        Assert.Equal(new Bucket(D(2026, 10, 1), Today, D(2026, 10, 1)), buckets[^1]);
    }

    [Fact]
    public void GetBuckets_LastMonth_IsOneDailyBucketPerDayOfTheWindowAcrossTheMonthBoundary()
    {
        var buckets = PeriodBucketer.GetBuckets(R(D(2026, 9, 10), Today), Granularity.Day);

        Assert.Equal(30, buckets.Count);
        Assert.Equal(new Bucket(D(2026, 9, 10), D(2026, 9, 10), D(2026, 9, 10)), buckets[0]);
        Assert.Equal(new Bucket(Today, Today, Today), buckets[^1]);
    }

    [Fact]
    public void GetBuckets_YearlyTimeline_HasOneBucketPerCalendarYearClippedToTheRange()
    {
        var buckets = PeriodBucketer.GetBuckets(R(D(2023, 10, 1), D(2026, 9, 30)), Granularity.Year);

        Assert.Equal(
            new[]
            {
                new Bucket(D(2023, 10, 1), D(2023, 12, 31), D(2023, 1, 1)),
                new Bucket(D(2024, 1, 1), D(2024, 12, 31), D(2024, 1, 1)),
                new Bucket(D(2025, 1, 1), D(2025, 12, 31), D(2025, 1, 1)),
                new Bucket(D(2026, 1, 1), D(2026, 9, 30), D(2026, 1, 1)),
            },
            buckets);
    }

    [Fact]
    public void GetBuckets_ASingleDayRange_IsOneBucket() =>
        Assert.Equal(new[] { new Bucket(Today, Today, D(2026, 10, 1)) }, PeriodBucketer.GetBuckets(R(Today, Today), Granularity.Month));

    [Theory]
    [InlineData(Granularity.Day)]
    [InlineData(Granularity.Month)]
    [InlineData(Granularity.Year)]
    public void GetBuckets_TileTheRangeWithNoGapsOrOverlap(Granularity granularity)
    {
        var range = R(D(2023, 10, 17), D(2026, 2, 3));

        var buckets = PeriodBucketer.GetBuckets(range, granularity);

        Assert.Equal(range.From, buckets[0].From);
        Assert.Equal(range.To, buckets[^1].To);
        for (var i = 1; i < buckets.Count; i++)
            Assert.Equal(buckets[i - 1].To.AddDays(1), buckets[i].From);
    }

    // ---- Label ----

    [Fact]
    public void Label_UsesTheBucketKeyNotItsClippedStart()
    {
        var partial = new Bucket(D(2025, 10, 10), D(2025, 10, 31), D(2025, 10, 1));

        Assert.Equal("2025-10", PeriodBucketer.Label(partial, Granularity.Month));
        Assert.Equal("2025", PeriodBucketer.Label(partial with { Key = D(2025, 1, 1) }, Granularity.Year));
        Assert.Equal("10-10", PeriodBucketer.Label(new Bucket(D(2025, 10, 10), D(2025, 10, 10), D(2025, 10, 10)), Granularity.Day));
    }

    [Fact]
    public void Label_IsIndependentOfTheCurrentCulture()
    {
        var saved = CultureInfo.CurrentCulture;
        try
        {
            foreach (var name in new[] { "de-DE", "ar-EG", "th-TH" })
            {
                CultureInfo.CurrentCulture = new CultureInfo(name);
                Assert.Equal("2026-09", PeriodBucketer.Label(new Bucket(D(2026, 9, 1), D(2026, 9, 30), D(2026, 9, 1)), Granularity.Month));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
        }
    }

    // ---- PeriodTotals ----

    [Fact]
    public void PeriodTotals_NetIsIncomeMinusExpenses()
    {
        Assert.Equal(300, new PeriodTotals(500, 200).NetMinor);
        Assert.Equal(-150, new PeriodTotals(50, 200).NetMinor);
    }

    [Fact]
    public void PeriodTotals_HasDataEvenWhenNetIsZero()
    {
        Assert.False(default(PeriodTotals).HasData);
        Assert.True(new PeriodTotals(100, 100).HasData);
    }
}
