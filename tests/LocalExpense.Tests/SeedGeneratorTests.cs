using LocalExpense.Cli;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class SeedGeneratorTests
{
    private static readonly DateOnly End = new(2026, 10, 9);

    // A one- or two-month span used to have its only (or first) month removed as "the middle" one.
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(30)]
    public void Generate_FillsEveryMonthOfAShortSpanWithValidRowsInsideIt(int months)
    {
        var first = new DateOnly(2026, 10, 1).AddMonths(-(months - 1));

        var rows = SeedGenerator.Generate(End, months, randomSeed: 42);

        Assert.NotEmpty(rows);
        Assert.All(rows, t =>
        {
            Assert.InRange(t.Date, first, End);
            Assert.Empty(TransactionService.Problems(t));
        });
        if (months < 3)
        {
            Assert.Equal(months, rows.Select(t => (t.Date.Year, t.Date.Month)).Distinct().Count());
        }
    }

    [Fact]
    public void Generate_LeavesTheMiddleMonthEmpty()
    {
        // August, September and October 2026; the middle one (index 3 / 2 = 1) is September.
        var rows = SeedGenerator.Generate(End, 3, randomSeed: 42);

        Assert.DoesNotContain(rows, t => t.Date.Month == 9);
        Assert.Contains(rows, t => t.Date.Month == 8);
        Assert.Contains(rows, t => t.Date.Month == 10);
    }

    [Fact]
    public void Generate_IsTheSameForTheSameSeed() =>
        Assert.Equal(
            SeedGenerator.Generate(End, 6, 7).Select(t => (t.Date, t.AmountMinor, t.Category, t.Note)),
            SeedGenerator.Generate(End, 6, 7).Select(t => (t.Date, t.AmountMinor, t.Category, t.Note)));
}
