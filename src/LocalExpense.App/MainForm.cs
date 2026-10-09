using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense;

public partial class MainForm : Form
{
    private readonly TransactionService? service;

    // Designer path: no service, so nothing is loaded. Production composes the overload below via DI.
    public MainForm()
    {
        InitializeComponent();
        // Via the column so it works even while the designer keeps transactionsGrid as a local.
        amountColumn.DataGridView?.CellFormatting += AmountCellFormatting;
    }

    public MainForm(TransactionService service) : this()
    {
        this.service = service;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (service is null)
        {
            return;
        }

        try
        {
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private async void addButton_Click(object? sender, EventArgs e)
    {
        addButton.Enabled = false;
        try
        {
            await AddTransactionAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        finally
        {
            addButton.Enabled = true;
        }
    }

    private async Task RefreshAsync()
    {
        transactionBindingSource.DataSource = await service!.GetAllAsync();
    }

    private async Task AddTransactionAsync()
    {
        var categories = await service!.GetCategoriesAsync();

        using var dialog = new AddTransactionForm();
        dialog.SetCategories(categories);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is not { } transaction)
        {
            return;
        }

        await service.AddAsync(transaction);
        await RefreshAsync();
    }

    private void ShowError(Exception ex) =>
        MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

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
}
