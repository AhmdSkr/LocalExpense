using System.Globalization;
using System.Text;
using LocalExpense.Models;

namespace LocalExpense.Services;

public sealed record CsvImportError(int Line, string Message);

/// <summary>Rows are only meaningful when <see cref="Errors"/> is empty; callers should refuse a partial import.</summary>
public sealed record CsvImportResult(IReadOnlyList<Transaction> Rows, IReadOnlyList<CsvImportError> Errors)
{
    public bool Succeeded => Errors.Count == 0;
}

/// <summary>
/// Reads RFC 4180 CSV into transactions. Pure parsing and validation: no file or database access, current culture ignored.
/// Columns are matched by header name, in any order: Date (yyyy-MM-dd), Category, Amount (signed decimal, at most
/// <see cref="Money.Exponent"/> decimals, not zero) and optionally Note. An Id column, as written by
/// <see cref="CsvExporter"/>, is ignored. Every row is checked and every problem reported, with its line number.
/// </summary>
public static class CsvImporter
{
    public static CsvImportResult Read(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var rows = new List<Transaction>();
        var errors = new List<CsvImportError>();
        (Dictionary<string, int> Map, int Count)? columns = null;

        foreach (var (line, fields) in ReadRecords(reader, errors))
        {
            if (columns is null)
            {
                columns = MapHeader(fields, line, errors);
                if (columns is null)
                {
                    break;
                }

                continue;
            }

            var row = ParseRow(fields, columns.Value.Map, columns.Value.Count, line, errors);
            if (row is not null)
            {
                rows.Add(row);
            }
        }

        if (columns is null && errors.Count == 0)
        {
            errors.Add(new CsvImportError(1, "The file is empty; expected a header row with Date, Category and Amount."));
        }

        return new CsvImportResult(rows, errors);
    }

    private static (Dictionary<string, int> Map, int Count)? MapHeader(List<string> header, int line, List<CsvImportError> errors)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Count; i++)
        {
            map[header[i].Trim().TrimStart('﻿')] = i;
        }

        var missing = new[] { "Date", "Category", "Amount" }.Where(name => !map.ContainsKey(name)).ToArray();
        if (missing.Length > 0)
        {
            errors.Add(new CsvImportError(line, $"The header must contain Date, Category and Amount (Note is optional); missing: {string.Join(", ", missing)}."));
            return null;
        }

        return (map, header.Count);
    }

    private static Transaction? ParseRow(List<string> fields, Dictionary<string, int> columns, int expectedFields, int line, List<CsvImportError> errors)
    {
        string Field(string name) => columns.TryGetValue(name, out var i) && i < fields.Count ? fields[i] : "";

        var problems = new List<string>();
        if (fields.Count != expectedFields)
        {
            problems.Add($"expected {expectedFields} fields but found {fields.Count}");
        }

        if (!DateOnly.TryParseExact(Field("Date").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            problems.Add("Date must be yyyy-MM-dd");
        }

        long? minor = null;
        if (!decimal.TryParse(Field("Amount").Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount)
            || decimal.Round(amount, Money.Exponent) != amount)
        {
            problems.Add($"Amount must be a number with at most {Money.Exponent} decimals, like -45.25");
        }
        else
        {
            try
            {
                minor = Money.ToMinor(amount);
            }
            catch (OverflowException)
            {
                minor = long.MaxValue;   // far beyond the limit, so the limit is what gets reported
            }
        }

        // From here on, the same rules (and messages) as TransactionService applies when saving.
        var row = new Transaction { Date = date, AmountMinor = minor ?? 0, Category = Unguard(Field("Category")), Note = Unguard(Field("Note")) };
        row.Normalize();
        string?[] ruleProblems =
        [
            minor is null ? null : TransactionService.AmountProblem(row.AmountMinor),
            TransactionService.CategoryProblem(row.Category),
            TransactionService.NoteProblem(row.Note),
        ];
        problems.AddRange(ruleProblems.OfType<string>());

        if (problems.Count > 0)
        {
            errors.Add(new CsvImportError(line, string.Join("; ", problems) + "."));
            return null;
        }

        return row;
    }

    /// <summary>Undoes the exporter's guard exactly: the leading apostrophe is removed only when the rest is text the exporter would have guarded.</summary>
    private static string Unguard(string text) =>
        text is ['\'', ..] && CsvExporter.NeedsFormulaGuard(text.AsSpan(1)) ? text[1..] : text;

    /// <summary>Yields each non-blank record with the line it starts on. Structural problems are added to <paramref name="errors"/>.</summary>
    private static IEnumerable<(int Line, List<string> Fields)> ReadRecords(TextReader reader, List<CsvImportError> errors)
    {
        var line = 1;
        var recordLine = 1;
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var closedQuote = false;
        var quotedAny = false;
        var reportedStray = false;

        while (true)
        {
            var next = reader.Read();
            if (next < 0)
            {
                if (inQuotes)
                {
                    errors.Add(new CsvImportError(recordLine, "A quoted field is never closed."));
                    yield break;
                }

                if (field.Length > 0 || fields.Count > 0 || quotedAny)
                {
                    fields.Add(field.ToString());
                    yield return (recordLine, fields);
                }

                yield break;
            }

            var c = (char)next;
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (reader.Peek() == '"')
                    {
                        reader.Read();
                        field.Append('"');
                    }
                    else
                    {
                        inQuotes = false;
                        closedQuote = true;
                    }
                }
                else
                {
                    if (c == '\n')
                    {
                        line++;
                    }

                    field.Append(c);
                }

                continue;
            }

            if (c == '"' && field.Length == 0 && !closedQuote)
            {
                inQuotes = true;
                quotedAny = true;
            }
            else if (c == ',')
            {
                fields.Add(field.ToString());
                field.Clear();
                closedQuote = false;
            }
            else if (c == '\n' || (c == '\r' && reader.Peek() == '\n'))
            {
                if (c == '\r')
                {
                    reader.Read();
                }

                fields.Add(field.ToString());
                var blank = fields.Count == 1 && fields[0].Length == 0 && !quotedAny;
                var finished = fields;
                var finishedLine = recordLine;
                fields = [];
                field.Clear();
                closedQuote = false;
                quotedAny = false;
                reportedStray = false;
                line++;
                recordLine = line;
                if (!blank)
                {
                    yield return (finishedLine, finished);
                }
            }
            else
            {
                if (closedQuote && !reportedStray)
                {
                    errors.Add(new CsvImportError(line, "Unexpected text after a closing quote."));
                    reportedStray = true;
                }

                field.Append(c);
            }
        }
    }
}
