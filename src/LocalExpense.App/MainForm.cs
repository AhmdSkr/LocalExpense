using System.Text;
using LocalExpense.Models;
using LocalExpense.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LocalExpense;

public partial class MainForm : Form
{
    private readonly TransactionService? service;
    private readonly IServiceScopeFactory? scopeFactory;
    private bool busy;
    private bool updatingFilter;
    private readonly LatestQuery loads = new();

    // Designer path: no service, so nothing is loaded. Production composes the overload below via DI.
    public MainForm()
    {
        InitializeComponent();
        Icon = FormHelpers.AppIcon;
        transactionsGrid.CellFormatting += AmountCellFormatting;
    }

    public MainForm(TransactionService service, IServiceScopeFactory scopeFactory) : this()
    {
        this.service = service;
        this.scopeFactory = scopeFactory;
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

    private void exportButton_Click(object? sender, EventArgs e) =>
        exportMenu.Show(exportButton, new Point(0, exportButton.Height));

    private async void reportsButton_Click(object? sender, EventArgs e) =>
        await RunGuardedAsync(ShowReports);

    private async void exportFilteredMenuItem_Click(object? sender, EventArgs e) =>
        await RunGuardedAsync(() => ExportAsync(filtered: true));

    private async void exportAllMenuItem_Click(object? sender, EventArgs e) =>
        await RunGuardedAsync(() => ExportAsync(filtered: false));

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
            await this.TryAsync(operation);
        }
        finally
        {
            busy = false;
            toolbarPanel.Enabled = true;
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
            await this.TryAsync(RefreshAsync);
        }
    }

    private DateOnly? FromDate => fromPicker.Checked ? DateOnly.FromDateTime(fromPicker.Value) : null;

    private DateOnly? ToDate => toPicker.Checked ? DateOnly.FromDateTime(toPicker.Value) : null;

    // Index 0 is the "All categories" entry, which means no restriction.
    private string? SelectedCategory => categoryFilter.SelectedIndex > 0 ? (string)categoryFilter.SelectedItem! : null;

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        loads.Cancel();
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

        if (from > to)
        {
            loads.Cancel();
            errorProvider.SetError(toPicker, Strings.EndBeforeStart);
            transactionBindingSource.DataSource = new List<Transaction>();
            return;
        }

        errorProvider.SetError(toPicker, "");
        if (await loads.RunAsync(ct => service!.GetFilteredAsync(from, to, category, ct)) is { } rows)
        {
            transactionBindingSource.DataSource = rows;
        }
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

    // Each open gets its own scope: the container keeps every IDisposable it resolves until its scope ends, so a form resolved from the root
    // would stay alive until the app exits. Disposing the scope disposes the form.
    private Task ShowReports()
    {
        if (scopeFactory is not null)
        {
            using var scope = scopeFactory.CreateScope();
            scope.ServiceProvider.GetRequiredService<ReportsForm>().ShowDialog(this);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Exports to CSV. The filter values are read before the dialog opens, and the rows come from the database
    /// (not the grid) after the file name is chosen, so the file reflects the data as of the save.
    /// </summary>
    private async Task ExportAsync(bool filtered)
    {
        if (service is null)
        {
            return;
        }

        var from = filtered ? FromDate : null;
        var to = filtered ? ToDate : null;
        var category = filtered ? SelectedCategory : null;
        if (from > to)
        {
            MessageBox.Show(this, Strings.EndBeforeStart, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = Strings.ExportTitle,
            Filter = Strings.CsvFileFilter,
            DefaultExt = "csv",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = CsvExporter.SuggestFileName(from, to, category),
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var path = dialog.FileName;
        var rows = filtered
            ? await service.GetFilteredAsync(from, to, category)
            : await service.GetAllAsync();
        try
        {
            await Task.Run(() => WriteCsvFile(path, rows));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            MessageBox.Show(this, string.Format(Strings.ExportFailed, path, ex.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MessageBox.Show(this, string.Format(Strings.ExportDone, rows.Count, path), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // Writes next to the target and moves into place, so a failure never leaves a half-written or truncated file.
    private static void WriteCsvFile(string path, IEnumerable<Transaction> rows)
    {
        var temp = Path.Combine(Path.GetDirectoryName(path)!, $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var writer = new StreamWriter(temp, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
            {
                CsvExporter.Write(rows, writer);
            }

            File.Move(temp, path, overwrite: true);
        }
        finally
        {
            File.Delete(temp);
        }
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
        e.CellStyle!.ForeColor = FormHelpers.AmountColor(minor);
        e.FormattingApplied = true;
    }
}
