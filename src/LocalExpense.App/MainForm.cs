using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense;

public partial class MainForm : Form
{
    private readonly TransactionService? service;
    private bool busy;
    private bool updatingFilter;
    private CancellationTokenSource? loadCts;

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
            await RunGuardedAsync(ReloadAsync);
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

    // Without a handler the grid shows its own modal "Default Error Dialog". A row can be painted
    // for an instant after the list behind it has already shrunk (rapid filter changes); that
    // IndexOutOfRangeException is transient, so it is ignored. Anything else is reported.
    private void transactionsGrid_DataError(object? sender, DataGridViewDataErrorEventArgs e)
    {
        e.ThrowException = false;
        if (e.Exception is not IndexOutOfRangeException)
        {
            MessageBox.Show(this, e.Exception?.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            await TryAsync(operation);
        }
        finally
        {
            busy = false;
            toolbarPanel.Enabled = true;
        }
    }

    private async Task TryAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void fromPicker_ValueChanged(object? sender, EventArgs e) =>
        await ApplyFilterAsync();

    private async void toPicker_ValueChanged(object? sender, EventArgs e) =>
        await ApplyFilterAsync();

    private async void categoryFilter_SelectedIndexChanged(object? sender, EventArgs e) =>
        await ApplyFilterAsync();

    private async void clearFilterButton_Click(object? sender, EventArgs e)
    {
        // Resetting each control fires its own change event; reload once afterwards instead.
        updatingFilter = true;
        fromPicker.Checked = false;
        toPicker.Checked = false;
        categoryFilter.SelectedIndex = categoryFilter.Items.Count > 0 ? 0 : -1;
        updatingFilter = false;
        await ApplyFilterAsync();
    }

    // Not routed through RunGuardedAsync: a change made while a query runs must not be dropped,
    // so a new reload cancels the one in flight and RefreshAsync shows only the newest result.
    private async Task ApplyFilterAsync()
    {
        if (!updatingFilter && service is not null)
        {
            await TryAsync(RefreshAsync);
        }
    }

    private DateOnly? FromDate => fromPicker.Checked ? DateOnly.FromDateTime(fromPicker.Value) : null;

    private DateOnly? ToDate => toPicker.Checked ? DateOnly.FromDateTime(toPicker.Value) : null;

    // Index 0 is the "All categories" entry, which means no restriction.
    private string? SelectedCategory => categoryFilter.SelectedIndex > 0 ? (string)categoryFilter.SelectedItem! : null;

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        CancelPendingLoad();
        base.OnFormClosed(e);
    }

    /// <summary>Reloads the category list, then the grid. Used on startup and after the data changes.</summary>
    private async Task ReloadAsync()
    {
        await LoadCategoriesAsync();
        await RefreshAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await service!.GetCategoriesAsync();
        var previous = SelectedCategory;

        // Rebuilding the list fires change events; the caller reloads the grid once afterwards.
        updatingFilter = true;
        try
        {
            categoryFilter.Items.Clear();
            categoryFilter.Items.Add(Strings.AllCategories);
            categoryFilter.Items.AddRange(categories.ToArray<object>());

            // Keep the chosen category if it still exists; otherwise fall back to "All categories".
            categoryFilter.SelectedIndex = previous is null ? 0 : Math.Max(0, categoryFilter.Items.IndexOf(previous));
        }
        finally
        {
            updatingFilter = false;
        }
    }

    private async Task RefreshAsync()
    {
        var from = FromDate;
        var to = ToDate;
        var category = SelectedCategory;
        CancelPendingLoad();

        if (from > to)
        {
            errorProvider.SetError(toPicker, Strings.EndBeforeStart);
            transactionBindingSource.DataSource = new List<Transaction>();
            return;
        }

        errorProvider.SetError(toPicker, "");
        var cts = loadCts = new CancellationTokenSource();
        List<Transaction> rows;
        try
        {
            rows = await service!.GetFilteredAsync(from, to, category, cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            return;
        }

        // The query can finish in the same instant it is cancelled, so check again before showing it.
        if (!cts.IsCancellationRequested)
        {
            transactionBindingSource.DataSource = rows;
        }
    }

    private void CancelPendingLoad()
    {
        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = null;
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
        await ReloadAsync();
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
            MessageBox.Show(this, Strings.TransactionGone, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        await ReloadAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var selected = SelectedTransactions();
        if (selected.Count == 0)
        {
            return;
        }

        var prompt = selected.Count == 1
            ? Strings.DeleteOnePrompt
            : string.Format(Strings.DeleteManyPrompt, selected.Count);
        if (MessageBox.Show(this, prompt, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        {
            return;
        }

        await service!.DeleteManyAsync(selected.Select(t => t.Id).ToArray());
        await ReloadAsync();
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
