using System.Globalization;

namespace LocalExpense.Models;

/// <summary>
/// The app's single hard-coded currency. Amounts are stored as integer minor units
/// (e.g. cents) in <see cref="Transaction.AmountMinor"/>; everything that converts to or
/// from a decimal or a display string goes through here.
/// Negative = expense, positive = income.
/// </summary>
public static class Money
{
    public const string Code = "USD";
    public const string Symbol = "$";

    /// <summary>Digits after the decimal point (2 for USD). Changing this after data exists rescales every stored amount.</summary>
    public const int Exponent = 2;

    private static readonly decimal Factor = (decimal)Math.Pow(10, Exponent);

    /// <summary>Converts a major-unit amount (12.34) to minor units (1234). Extra decimals are rounded away from zero.</summary>
    public static long ToMinor(decimal major) =>
        checked((long)decimal.Round(major * Factor, 0, MidpointRounding.AwayFromZero));

    public static decimal ToMajor(long minor) => minor / Factor;

    public static string Format(long minor) =>
        (minor < 0 ? "-" : "") + Symbol + Math.Abs(ToMajor(minor)).ToString($"N{Exponent}", CultureInfo.InvariantCulture);
}
