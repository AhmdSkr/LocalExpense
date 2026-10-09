using System.Globalization;
using LocalExpense.Models;

namespace LocalExpense.Services;

/// <summary>
/// Writes transactions as RFC 4180 CSV. Pure formatting: it keeps the order it is given, never touches the
/// file system, and ignores the current culture. The caller picks the encoding (the app uses UTF-8 with BOM).
/// </summary>
public static class CsvExporter
{
    public const string Header = "Id,Date,Category,Amount,Note";

    private const string RecordSeparator = "\r\n";
    private const string DateFormat = "yyyy-MM-dd";
    private static readonly string AmountFormat = $"F{Money.Exponent}";

    /// <summary>
    /// Writes the header and one record per transaction, each ended by CRLF. Amount is the signed major-unit
    /// value (-45.25). Category and Note that start with = + - @ tab or CR get a leading apostrophe so a spreadsheet
    /// shows them as text instead of evaluating a formula (see <see cref="NeedsFormulaGuard"/>). The apostrophe stays visible
    /// in a spreadsheet; <see cref="CsvImporter"/> removes it again.
    /// </summary>
    public static void Write(IEnumerable<Transaction> transactions, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(Header);
        writer.Write(RecordSeparator);
        foreach (var t in transactions)
        {
            writer.Write(t.Id.ToString(CultureInfo.InvariantCulture));
            writer.Write(',');
            writer.Write(t.Date.ToString(DateFormat, CultureInfo.InvariantCulture));
            writer.Write(',');
            writer.Write(Escape(GuardFormula(t.Category)));
            writer.Write(',');
            writer.Write(Money.ToMajor(t.AmountMinor).ToString(AmountFormat, CultureInfo.InvariantCulture));
            writer.Write(',');
            writer.Write(Escape(GuardFormula(t.Note)));
            writer.Write(RecordSeparator);
        }
    }

    /// <summary>
    /// A file name that shows the filters used, e.g. transactions-Food-2025-01-01-to-2025-03-31.csv.
    /// No filter at all gives transactions-all.csv. Characters a file name cannot hold become '_'.
    /// </summary>
    public static string SuggestFileName(DateOnly? from, DateOnly? to, string? category)
    {
        var name = new System.Text.StringBuilder("transactions");
        if (category is not null)
        {
            name.Append('-').Append(SanitizeForFileName(category));
        }

        if (from is { } start && to is { } end)
        {
            name.Append('-').Append(Format(start)).Append("-to-").Append(Format(end));
        }
        else if (from is { } onlyStart)
        {
            name.Append("-from-").Append(Format(onlyStart));
        }
        else if (to is { } onlyEnd)
        {
            name.Append("-until-").Append(Format(onlyEnd));
        }

        if (category is null && from is null && to is null)
        {
            name.Append("-all");
        }

        return name.Append(".csv").ToString();

        static string Format(DateOnly d) => d.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    }

    private static string SanitizeForFileName(string text)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = text.Trim().Select(c => char.IsControl(c) || Array.IndexOf(invalid, c) >= 0 ? '_' : c).ToArray();
        return new string(chars);
    }

    /// <summary>
    /// True when the text starts with a formula character, or with an apostrophe in front of text that is itself guarded
    /// ('=x). Guarding the second kind too is what lets the importer undo every guard exactly, so '=x survives a round trip.
    /// </summary>
    internal static bool NeedsFormulaGuard(ReadOnlySpan<char> text) =>
        text is [('=' or '+' or '-' or '@' or '\t' or '\r'), ..] || (text is ['\'', ..] && NeedsFormulaGuard(text[1..]));

    private static string GuardFormula(string? text) =>
        text is null ? string.Empty : NeedsFormulaGuard(text) ? "'" + text : text;

    private static string Escape(string field) =>
        field.AsSpan().IndexOfAny(",\"\r\n") >= 0 ? "\"" + field.Replace("\"", "\"\"") + "\"" : field;
}
