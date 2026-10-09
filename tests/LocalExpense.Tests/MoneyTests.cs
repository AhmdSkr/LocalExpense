using System.Globalization;
using LocalExpense.Models;

namespace LocalExpense.Tests;

// These tests pin the current currency config (USD, exponent 2).
// If you change Money.Code/Symbol/Exponent, the expected values below must change too.
public class MoneyTests
{
    [Theory]
    [InlineData(12.34, 1234)]
    [InlineData(0.01, 1)]
    [InlineData(100, 10000)]
    [InlineData(0, 0)]
    public void ToMinor_ConvertsMajorUnitsToCents(double major, long expected) =>
        Assert.Equal(expected, Money.ToMinor((decimal)major));

    [Fact]
    public void ToMinor_KeepsTheSignForExpenses() =>
        Assert.Equal(-1234, Money.ToMinor(-12.34m));

    [Theory]
    [InlineData("0.005", 1)]    // midpoint rounds away from zero (not banker's rounding)
    [InlineData("-0.005", -1)]
    [InlineData("0.015", 2)]    // banker's rounding would give 2 here too; 0.025 below tells them apart
    [InlineData("0.025", 3)]    // banker's rounding would give 2
    [InlineData("0.004", 0)]    // below the midpoint rounds to zero
    [InlineData("-0.004", 0)]
    [InlineData("12.345", 1235)]
    public void ToMinor_RoundsExtraDecimalsAwayFromZero(string major, long expected) =>
        Assert.Equal(expected, Money.ToMinor(decimal.Parse(major, CultureInfo.InvariantCulture)));

    [Fact]
    public void ToMinor_ThrowsInsteadOfWrappingOnOverflow() =>
        // 1e17 * 100 = 1e19, which is above long.MaxValue (~9.22e18)
        Assert.Throws<OverflowException>(() => Money.ToMinor(100_000_000_000_000_000m));

    [Fact]
    public void ToMajor_ConvertsCentsBackToDecimal()
    {
        Assert.Equal(12.34m, Money.ToMajor(1234));
        Assert.Equal(-0.05m, Money.ToMajor(-5));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("12.34")]
    [InlineData("-12.34")]
    [InlineData("999999.99")]
    [InlineData("0")]
    public void RoundTrip_ToMinorThenToMajor_ReturnsTheOriginal(string major)
    {
        var original = decimal.Parse(major, CultureInfo.InvariantCulture);
        Assert.Equal(original, Money.ToMajor(Money.ToMinor(original)));
    }

    [Theory]
    [InlineData(123456, "$1,234.56")]
    [InlineData(-5, "-$0.05")]
    [InlineData(0, "$0.00")]
    [InlineData(100, "$1.00")]
    [InlineData(-1234567, "-$12,345.67")]
    public void Format_ShowsSymbolSignAndTwoDecimals(long minor, string expected) =>
        Assert.Equal(expected, Money.Format(minor));

    [Theory]
    [InlineData("de-DE")]   // uses '.' for thousands and ',' for decimals
    [InlineData("fr-FR")]   // uses a narrow no-break space for thousands
    [InlineData("ar-EG")]   // may use different digits/separators
    public void Format_IgnoresTheCurrentCulture(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Assert.Equal("$1,234.56", Money.Format(123456));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
