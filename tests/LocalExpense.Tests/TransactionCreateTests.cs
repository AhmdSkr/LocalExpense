using LocalExpense.Models;

namespace LocalExpense.Tests;

// Pins the current currency config (USD, exponent 2), like MoneyTests.
public class TransactionCreateTests
{
    private static readonly DateOnly Day = new(2026, 10, 9);

    [Fact]
    public void Create_Expense_StoresANegativeAmount() =>
        Assert.Equal(-1234, Transaction.Create(Day, 12.34m, isExpense: true, "Food", null).AmountMinor);

    [Fact]
    public void Create_Income_StoresAPositiveAmount() =>
        Assert.Equal(1234, Transaction.Create(Day, 12.34m, isExpense: false, "Salary", null).AmountMinor);

    [Fact]
    public void Create_CopiesTheDate() =>
        Assert.Equal(Day, Transaction.Create(Day, 1m, isExpense: true, "Food", null).Date);

    [Fact]
    public void Create_TrimsCategoryAndNote()
    {
        var t = Transaction.Create(Day, 1m, isExpense: true, "  Food  ", "  lunch  ");

        Assert.Equal("Food", t.Category);
        Assert.Equal("lunch", t.Note);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_StoresABlankNoteAsNull(string? note) =>
        Assert.Null(Transaction.Create(Day, 1m, isExpense: true, "Food", note).Note);

    [Fact]
    public void Create_LeavesIdUnset() =>
        Assert.Equal(0, Transaction.Create(Day, 1m, isExpense: true, "Food", null).Id);

    [Fact]
    public void Create_ThrowsInsteadOfWrappingOnOverflow() =>
        Assert.Throws<OverflowException>(() =>
            Transaction.Create(Day, 100_000_000_000_000_000m, isExpense: true, "Food", null));
}
