using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense.Tests;

public class CsvImporterTests
{
    private static CsvImportResult Read(string csv) => CsvImporter.Read(new StringReader(csv));

    private static List<Transaction> Rows(string csv)
    {
        var result = Read(csv);
        Assert.Empty(result.Errors);
        return result.Rows.ToList();
    }

    [Fact]
    public void Read_ParsesAllColumns()
    {
        var rows = Rows("Date,Category,Amount,Note\r\n2025-03-02,Food,-45.25,lunch\r\n2025-03-03,Salary,1500,\r\n");

        Assert.Equal(2, rows.Count);
        Assert.Equal(new DateOnly(2025, 3, 2), rows[0].Date);
        Assert.Equal(-4525, rows[0].AmountMinor);
        Assert.Equal("Food", rows[0].Category);
        Assert.Equal("lunch", rows[0].Note);
        Assert.Equal(150000, rows[1].AmountMinor);
        Assert.Null(rows[1].Note);
    }

    [Fact]
    public void Read_NoteColumnIsOptional()
    {
        var rows = Rows("Date,Category,Amount\n2025-03-02,Food,-1.50\n");

        Assert.Equal(-150, Assert.Single(rows).AmountMinor);
        Assert.Null(rows[0].Note);
    }

    [Fact]
    public void Read_MatchesColumnsByNameInAnyOrderAndIgnoresId()
    {
        var rows = Rows("Note,Id,Amount,Category,Date\nhi,7,2.00,Food,2025-01-01\n");

        var row = Assert.Single(rows);
        Assert.Equal(0, row.Id);
        Assert.Equal(("Food", 200L, "hi"), (row.Category, row.AmountMinor, row.Note));
    }

    [Fact]
    public void Read_AcceptsTheExportersOwnOutput()
    {
        var exported = new StringWriter();
        CsvExporter.Write(
        [
            new Transaction { Id = 2, Date = new DateOnly(2025, 3, 2), AmountMinor = -4525, Category = "Food, drink", Note = "say \"hi\"\r\nnext" },
            new Transaction { Id = 1, Date = new DateOnly(2025, 3, 1), AmountMinor = 100, Category = "=odd", Note = "-5 refund" },
            new Transaction { Id = 3, Date = new DateOnly(2025, 3, 3), AmountMinor = 1, Category = "'tis", Note = "'=SUM(A1)" },
        ], exported);

        var rows = Rows(exported.ToString());

        Assert.Equal(3, rows.Count);
        Assert.Equal(("Food, drink", -4525L, "say \"hi\"\r\nnext"), (rows[0].Category, rows[0].AmountMinor, rows[0].Note));
        Assert.Equal(("=odd", 100L, "-5 refund"), (rows[1].Category, rows[1].AmountMinor, rows[1].Note));
        Assert.Equal(("'tis", "'=SUM(A1)"), (rows[2].Category, rows[2].Note));   // a typed apostrophe survives too
    }

    [Theory]
    [InlineData("'=SUM(A1)", "=SUM(A1)")]
    [InlineData("'-5 refund", "-5 refund")]
    [InlineData("'@x", "@x")]
    [InlineData("'tis the season", "'tis the season")]   // an apostrophe that guards nothing stays
    [InlineData("''=x", "'=x")]                          // the exporter's guard in front of a typed '=x
    [InlineData("'''=x", "''=x")]
    [InlineData("a'=b", "a'=b")]
    public void Read_RemovesTheApostropheOnlyWhenItGuardsAFormulaCharacter(string written, string expected)
    {
        var rows = Rows($"Date,Category,Amount,Note\n2025-01-01,Food,1.00,\"{written}\"\n");

        Assert.Equal(expected, rows[0].Note);
    }

    [Fact]
    public void Read_RemovesTheGuardFromCategoryToo() =>
        Assert.Equal("=odd", Rows("Date,Category,Amount,Note\n2025-01-01,'=odd,1.00,\n")[0].Category);

    [Fact]
    public void Read_HandlesQuotedCommasQuotesAndNewlines()
    {
        var rows = Rows("Date,Category,Amount,Note\r\n2025-01-01,\"Food, drink\",-1.00,\"he said \"\"hi\"\"\nsecond line\"\r\n2025-01-02,Other,-2.00,x\r\n");

        Assert.Equal("Food, drink", rows[0].Category);
        Assert.Equal("he said \"hi\"\nsecond line", rows[0].Note);
        Assert.Equal(new DateOnly(2025, 1, 2), rows[1].Date);
    }

    [Fact]
    public void Read_ToleratesBomLfEndingsBlankLinesAndNoFinalNewline()
    {
        var rows = Rows("﻿Date,Category,Amount,Note\n\n2025-01-01,Food,-1.00,a\n\n2025-01-02,Food,-2.00,b");

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void Read_TrimsCategoryAndNote()
    {
        var row = Assert.Single(Rows("Date,Category,Amount,Note\n2025-01-01,  Food  ,-1.00,  hi  \n"));

        Assert.Equal(("Food", "hi"), (row.Category, row.Note));
    }

    [Fact]
    public void Read_HeaderOnlyGivesNoRowsAndNoErrors()
    {
        var result = Read("Date,Category,Amount,Note\r\n");

        Assert.Empty(result.Rows);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Read_EmptyInputIsAnError()
    {
        var result = Read("");

        Assert.Contains("empty", Assert.Single(result.Errors).Message);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void Read_MissingRequiredColumnsIsOneClearError()
    {
        var result = Read("Date,Amount\n2025-01-01,1.00\n");

        var error = Assert.Single(result.Errors);
        Assert.Equal(1, error.Line);
        Assert.Contains("Category", error.Message);
        Assert.Empty(result.Rows);
    }

    [Theory]
    [InlineData("2025-1-1", "Date")]
    [InlineData("01/02/2025", "Date")]
    [InlineData("2025-02-30", "Date")]
    [InlineData("", "Date")]
    public void Read_RejectsMalformedDates(string date, string field)
    {
        var result = Read($"Date,Category,Amount,Note\n{date},Food,1.00,\n");

        Assert.Contains(field, Assert.Single(result.Errors).Message);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1,5")]       // a decimal comma is not invariant
    [InlineData("1.005")]     // would silently round
    [InlineData("0")]
    [InlineData("0.00")]
    [InlineData("")]
    [InlineData("1e3")]
    [InlineData("99999999999999999999")]
    [InlineData("1000000000.00")]   // just past TransactionService.MaxAmountMinor
    [InlineData("-1000000000.00")]
    public void Read_RejectsBadAmounts(string amount)
    {
        var result = Read($"Date,Category,Amount,Note\n2025-01-01,Food,\"{amount}\",\n");

        Assert.Contains("Amount", Assert.Single(result.Errors).Message);
    }

    [Theory]
    [InlineData("-45.25", -4525)]
    [InlineData("+45.25", 4525)]
    [InlineData("45", 4500)]
    [InlineData("0.5", 50)]
    [InlineData("-0.05", -5)]
    [InlineData("1234.50", 123450)]
    [InlineData("999999999.99", TransactionService.MaxAmountMinor)]
    [InlineData("-999999999.99", -TransactionService.MaxAmountMinor)]
    public void Read_ParsesAmountsInInvariantFormat(string amount, long expected) =>
        Assert.Equal(expected, Rows($"Date,Category,Amount,Note\n2025-01-01,Food,{amount},\n")[0].AmountMinor);

    [Fact]
    public void Read_RejectsMissingAndOverlongCategoryAndNote()
    {
        var csv = "Date,Category,Amount,Note\n"
            + "2025-01-01,,1.00,\n"
            + $"2025-01-01,{new string('c', TransactionService.MaxCategoryLength + 1)},1.00,\n"
            + $"2025-01-01,Food,1.00,{new string('n', TransactionService.MaxNoteLength + 1)}\n"
            + $"2025-01-01,{new string('c', TransactionService.MaxCategoryLength)},1.00,{new string('n', TransactionService.MaxNoteLength)}\n";

        var result = Read(csv);

        Assert.Equal(new[] { 2, 3, 4 }, result.Errors.Select(e => e.Line));
        Assert.Single(result.Rows);
    }

    [Fact]
    public void Read_ReportsEveryBadRowWithItsLineNumber()
    {
        var result = Read("Date,Category,Amount,Note\n2025-01-01,Food,1.00,ok\nbad,Food,1.00,\n2025-01-03,Food,x,\n2025-01-04,Food,1.00,ok\n");

        Assert.Equal(new[] { 3, 4 }, result.Errors.Select(e => e.Line));
        Assert.Equal(2, result.Rows.Count);
    }

    [Fact]
    public void Read_LineNumbersCountNewlinesInsideQuotedFields()
    {
        var result = Read("Date,Category,Amount,Note\n2025-01-01,Food,1.00,\"a\nb\nc\"\nbad,Food,1.00,\n");

        Assert.Equal(5, Assert.Single(result.Errors).Line);
    }

    [Theory]
    [InlineData("2025-01-01,Food,1.00")]            // too few fields
    [InlineData("2025-01-01,Food,1.00,a,b")]        // too many
    public void Read_RejectsWrongFieldCounts(string row)
    {
        var result = Read($"Date,Category,Amount,Note\n{row}\n");

        Assert.Contains("fields", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public void Read_ReportsAnUnclosedQuote()
    {
        var result = Read("Date,Category,Amount,Note\n2025-01-01,Food,1.00,\"never closed\n");

        Assert.Contains("never closed", Assert.Single(result.Errors).Message);
        Assert.Equal(2, result.Errors[0].Line);
    }

    [Fact]
    public void Read_ReportsTextAfterAClosingQuote()
    {
        var result = Read("Date,Category,Amount,Note\n2025-01-01,\"Food\"x,1.00,\n");

        Assert.Contains("closing quote", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public void Read_RejectsNullReader() =>
        Assert.Throws<ArgumentNullException>(() => CsvImporter.Read(null!));
}
