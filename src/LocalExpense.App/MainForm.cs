using LocalExpense.Models;

namespace LocalExpense;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        transactionBindingSource.DataSource = SampleData();
        // Via the column so it works even while the designer keeps transactionsGrid as a local.
        amountColumn.DataGridView?.CellFormatting += AmountCellFormatting;
    }

    private void AmountCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != amountColumn.Index || e.Value is not long minor)
        {
            return;
        }

        e.Value = Money.Format(minor);
        e.CellStyle!.ForeColor = minor < 0 ? Color.Firebrick : Color.SeaGreen;
        e.FormattingApplied = true;
    }

    // Temporary hardcoded rows; replaced by TransactionService once the form loads real data.
    private static List<Transaction> SampleData() =>
    [
        Row(2026, 10, 1, 350_000, "Salary", "October pay"),
        Row(2026, 10, 1, -120_000, "Rent", "October rent"),
        Row(2026, 10, 2, -4_525, "Food", "Groceries"),
        Row(2026, 10, 3, -1_250, "Food", "Lunch"),
        Row(2026, 10, 3, -2_999, "Entertainment", "Streaming subscription"),
        Row(2026, 10, 5, -6_800, "Transport", "Monthly pass"),
        Row(2026, 10, 6, -1_875, "Food", null),
        Row(2026, 10, 7, 12_000, "Freelance", "Logo design"),
        Row(2026, 10, 8, -15_990, "Shopping", "Headphones"),
        Row(2026, 10, 8, 4_500, "Shopping", "Refund for returned item"),
        Row(2026, 10, 9, -3_200, "Health", "Pharmacy"),
        Row(2026, 10, 9, -950, "Food", "Coffee"),
    ];

    private static Transaction Row(int year, int month, int day, long amountMinor, string category, string? note) => new()
    {
        Date = new DateOnly(year, month, day),
        AmountMinor = amountMinor,
        Category = category,
        Note = note,
    };
}
