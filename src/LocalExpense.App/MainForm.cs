using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense;

public partial class MainForm : Form
{
    private readonly TransactionService? service;
    private bool busy;

    // Designer path: no service, so nothing is loaded. Production composes the overload below via DI.
    public MainForm()
    {
        InitializeComponent();
        transactionsGrid.CellFormatting += AmountCellFormatting;
    }

    public MainForm(TransactionService service) : this()
    {
        this.service = service;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (service is not null)
        {
            await RunGuardedAsync(RefreshAsync);
        }
    }

    private async void addButton_Click(object? sender, EventArgs e) =>
        await RunGuardedAsync(AddTransactionAsync);

    private async void editButton_Click(object? sender, EventArgs e) =>
        await RunGuardedAsync(EditSelectedAsync);

    private async void deleteButton_Click(object? sender, EventArgs e) =>
        await RunGuardedAsync(DeleteSelectedAsync);

    private async void transactionsGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            await RunGuardedAsync(EditSelectedAsync);
        }
    }

    private void transactionsGrid_SelectionChanged(object? sender, EventArgs e) 
    {
        var selected = transactionsGrid.SelectedRows.Count;
        editButton.Enabled = selected == 1;
        deleteButton.Enabled = selected >= 1;
    }

    /// <summary>Runs one toolbar operation: ignores re-entry, locks the toolbar meanwhile, and reports failures.</summary>
    private async Task RunGuardedAsync(Func<Task> operation)
    {
        if (busy)
        {
            return;
        }

        busy = true;
        toolbarPanel.Enabled = false;
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            busy = false;
            toolbarPanel.Enabled = true;
        }
    }

    private async Task RefreshAsync()
    {
        transactionBindingSource.DataSource = await service!.GetAllAsync();
    }

    private async Task AddTransactionAsync()
    {
        using var dialog = new TransactionForm();
        dialog.SetCategories(await service!.GetCategoriesAsync());
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is not { } transaction)
        {
            return;
        }

        await service.AddAsync(transaction);
        await RefreshAsync();
    }

    private async Task EditSelectedAsync()
    {
        if (SelectedTransactions() is not [var original])
        {
            return;
        }

        using var dialog = new TransactionForm();
        dialog.SetCategories(await service!.GetCategoriesAsync());
        dialog.SetTransaction(original);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is not { } edited)
        {
            return;
        }

        if (!await service.UpdateAsync(edited))
        {
            MessageBox.Show(this, "That transaction no longer exists.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        await RefreshAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var selected = SelectedTransactions();
        if (selected.Count == 0)
        {
            return;
        }

        var prompt = selected.Count == 1
            ? "Delete the selected transaction?"
            : $"Delete the {selected.Count} selected transactions?";
        if (MessageBox.Show(this, prompt, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        {
            return;
        }

        await service!.DeleteManyAsync(selected.Select(t => t.Id).ToArray());
        await RefreshAsync();
    }

    private List<Transaction> SelectedTransactions() =>
        transactionsGrid.SelectedRows.Cast<DataGridViewRow>()
            .Select(row => row.DataBoundItem)
            .OfType<Transaction>()
            .ToList();

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
