using System.Globalization;
using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class CsvExporterTests
{
    private static Transaction Tx(int id, string date, long minor, string category = "Food", string? note = null) => new()
    {
        Id = id,
        Date = DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
        AmountMinor = minor,
        Category = category,
        Note = note,
    };

    private static string Export(params Transaction[] rows)
    {
        var writer = new StringWriter();
        CsvExporter.Write(rows, writer);
        return writer.ToString();
    }

    [Fact]
    public void Write_EmptyList_WritesOnlyTheHeader() =>
        Assert.Equal("Id,Date,Category,Amount,Note\r\n", Export());

    [Fact]
    public void Write_WritesOneCrlfTerminatedRecordPerTransactionInTheGivenOrder()
    {
        var csv = Export(
            Tx(2, "2025-03-02", -4525, "Food", "lunch"),
            Tx(1, "2025-03-01", 150000, "Salary"));

        Assert.Equal(
            "Id,Date,Category,Amount,Note\r\n" +
            "2,2025-03-02,Food,-45.25,lunch\r\n" +
            "1,2025-03-01,Salary,1500.00,\r\n",
            csv);
    }

    [Theory]
    [InlineData(5, "0.05")]
    [InlineData(-5, "-0.05")]
    [InlineData(100, "1.00")]
    [InlineData(123456789, "1234567.89")]   // no thousands separator
    public void Write_AmountIsSignedMajorUnitsWithTwoDecimals(long minor, string expected) =>
        Assert.Contains($",{expected},", Export(Tx(1, "2025-01-01", minor)));

    [Fact]
    public void Write_NullNoteIsAnEmptyField() =>
        Assert.EndsWith("Food,1.00,\r\n", Export(Tx(1, "2025-01-01", 100, note: null)));

    [Theory]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("say \"hi\"", "\"say \"\"hi\"\"\"")]
    [InlineData("line1\nline2", "\"line1\nline2\"")]
    [InlineData("line1\r\nline2", "\"line1\r\nline2\"")]
    [InlineData("plain", "plain")]
    public void Write_QuotesFieldsPerRfc4180(string note, string expectedField) =>
        Assert.EndsWith($",1.00,{expectedField}\r\n", Export(Tx(1, "2025-01-01", 100, note: note)));

    [Fact]
    public void Write_QuotesCategoryToo() =>
        Assert.Contains(",\"Food, drink\",", Export(Tx(1, "2025-01-01", 100, "Food, drink")));

    [Theory]
    [InlineData("=SUM(A1)", "'=SUM(A1)")]
    [InlineData("+1", "'+1")]
    [InlineData("-5 refund", "'-5 refund")]
    [InlineData("@cmd", "'@cmd")]
    [InlineData("\tx", "'\tx")]
    [InlineData("a=b", "a=b")]            // only the first character matters
    [InlineData("'=x", "''=x")]           // looks like a guard already, so it is guarded again and imports back unchanged
    [InlineData("'tis", "'tis")]          // an apostrophe in front of plain text is left alone
    [InlineData("", "")]
    public void Write_GuardsNoteAgainstSpreadsheetFormulas(string note, string expectedField) =>
        Assert.EndsWith($",1.00,{expectedField}\r\n", Export(Tx(1, "2025-01-01", 100, note: note)));

    [Fact]
    public void Write_GuardsCategoryAndQuotesItWhenTheGuardedTextNeedsIt() =>
        Assert.Contains(",\"'=1,2\",", Export(Tx(1, "2025-01-01", 100, "=1,2")));

    [Fact]
    public void Write_GuardsAfterCarriageReturnAndQuotesTheResult() =>
        Assert.EndsWith(",1.00,\"'\rx\"\r\n", Export(Tx(1, "2025-01-01", 100, note: "\rx")));

    [Fact]
    public void Write_DoesNotGuardTheNumericAmount() =>
        Assert.Contains(",-45.25,", Export(Tx(1, "2025-01-01", -4525)));

    [Theory]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("ar-EG")]   // different digits, and a different default calendar for dates
    public void Write_IgnoresTheCurrentCulture(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Assert.Equal(
                "Id,Date,Category,Amount,Note\r\n12,2025-03-02,Food,-1234.50,n\r\n",
                Export(Tx(12, "2025-03-02", -123450, note: "n")));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Write_RejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => CsvExporter.Write(null!, new StringWriter()));
        Assert.Throws<ArgumentNullException>(() => CsvExporter.Write([], null!));
    }

    // A minimal RFC 4180 reader, independent of the writer, to prove the output parses back to the same fields.
    private static List<List<string>> Parse(string csv)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new System.Text.StringBuilder();
        var quoted = false;
        for (var i = 0; i < csv.Length; i++)
        {
            var c = csv[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < csv.Length && csv[i + 1] == '"') { field.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else field.Append(c);
            }
            else if (c == '"') quoted = true;
            else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
            else if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') { row.Add(field.ToString()); field.Clear(); rows.Add(row); row = []; i++; }
            else field.Append(c);
        }

        Assert.False(quoted, "unterminated quote");
        Assert.Equal(0, field.Length);
        Assert.Empty(row);
        return rows;
    }

    [Fact]
    public void Write_OutputParsesBackToTheSameFields()
    {
        var csv = Export(
            Tx(1, "2025-01-01", -4525, "Food, drink", "say \"hi\"\r\nnext,line"),
            Tx(2, "2025-01-02", 100, "Café €", null),
            Tx(3, "2025-01-03", 5, "=x", "plain"));

        var rows = Parse(csv);

        Assert.Equal(4, rows.Count);
        Assert.All(rows, r => Assert.Equal(5, r.Count));
        Assert.Equal(new[] { "1", "2025-01-01", "Food, drink", "-45.25", "say \"hi\"\r\nnext,line" }, rows[1]);
        Assert.Equal(new[] { "2", "2025-01-02", "Café €", "1.00", "" }, rows[2]);
        Assert.Equal(new[] { "3", "2025-01-03", "'=x", "0.05", "plain" }, rows[3]);
    }

    [Fact]
    public async Task Write_AfterTheServiceQuery_IsNewestFirstAndRespectsTheFilter()
    {
        using var factory = new TestDbFactory();
        var service = new TransactionService(factory);
        await service.AddAsync(new Transaction { Date = D("2025-01-10"), AmountMinor = -100, Category = "Food" });
        await service.AddAsync(new Transaction { Date = D("2025-03-01"), AmountMinor = -200, Category = "Food" });
        await service.AddAsync(new Transaction { Date = D("2025-03-01"), AmountMinor = -300, Category = "Food" });
        await service.AddAsync(new Transaction { Date = D("2025-02-01"), AmountMinor = -400, Category = "Other" });

        var all = Parse(Export((await service.GetAllAsync()).ToArray()));
        var filtered = Parse(Export((await service.GetFilteredAsync(D("2025-02-01"), null, "Food")).ToArray()));

        // Same date: higher Id first, matching the grid.
        Assert.Equal(new[] { "3", "2", "4", "1" }, all.Skip(1).Select(r => r[0]));
        Assert.Equal(new[] { "3", "2" }, filtered.Skip(1).Select(r => r[0]));
    }

    private static DateOnly D(string s) => DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    [Fact]
    public void SuggestFileName_NoFilterIsAll() =>
        Assert.Equal("transactions-all.csv", CsvExporter.SuggestFileName(null, null, null));

    [Fact]
    public void SuggestFileName_CategoryAndRange() =>
        Assert.Equal("transactions-Food-20250101-to-20250331.csv",
            CsvExporter.SuggestFileName(D("2025-01-01"), D("2025-03-31"), "Food"));

    [Fact]
    public void SuggestFileName_OpenEndedRanges()
    {
        Assert.Equal("transactions-from-20250101.csv", CsvExporter.SuggestFileName(D("2025-01-01"), null, null));
        Assert.Equal("transactions-until-20250331.csv", CsvExporter.SuggestFileName(null, D("2025-03-31"), null));
    }

    [Fact]
    public void SuggestFileName_CategoryOnly() =>
        Assert.Equal("transactions-Food.csv", CsvExporter.SuggestFileName(null, null, "Food"));

    [Fact]
    public void SuggestFileName_ReplacesCharactersAFileNameCannotHold() =>
        Assert.Equal("transactions-a_b_c_d.csv", CsvExporter.SuggestFileName(null, null, "a/b:c	d"));

    [Fact]
    public void SuggestFileName_IgnoresTheCurrentCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-EG");
            Assert.Equal("transactions-from-20250101.csv", CsvExporter.SuggestFileName(D("2025-01-01"), null, null));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
