using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense;

/// <summary>
/// Modal dialog that collects the fields of a transaction, for adding a new one or (after
/// <see cref="SetTransaction"/>) editing an existing one. It never touches the database:
/// on <see cref="DialogResult.OK"/> the caller reads <see cref="Result"/> and saves it.
/// </summary>
public partial class TransactionForm : Form
{
    private int editingId;

    public TransactionForm()
    {
        InitializeComponent();
        Icon = FormHelpers.AppIcon;

        // Must run before SetTransaction can assign Value, or a large amount exceeds the default Maximum.
        amountInput.DecimalPlaces = Money.Exponent;
        amountInput.Maximum = Money.ToMajor(TransactionService.MaxAmountMinor);   // the same limit the service and the importer enforce
        categoryCombo.MaxLength = TransactionService.MaxCategoryLength;
        noteText.MaxLength = TransactionService.MaxNoteLength;
        datePicker.Value = DateTime.Today;
    }

    /// <summary>The validated transaction; null unless the dialog was closed with OK. Carries the edited Id, or 0 when adding.</summary>
    public Transaction? Result { get; private set; }

    public void SetCategories(IEnumerable<string> categories)
    {
        categoryCombo.Items.Clear();
        categoryCombo.Items.AddRange(categories.ToArray<object>());
    }

    /// <summary>Switches the dialog to edit mode, prefilled from <paramref name="transaction"/>.</summary>
    public void SetTransaction(Transaction transaction)
    {
        editingId = transaction.Id;
        Text = Strings.EditTransactionTitle;
        datePicker.Value = transaction.Date.ToDateTime(TimeOnly.MinValue);
        expenseRadio.Checked = transaction.AmountMinor < 0;
        incomeRadio.Checked = transaction.AmountMinor >= 0;
        amountInput.Value = Money.ToMajor(Math.Abs(transaction.AmountMinor));
        categoryCombo.Text = transaction.Category;
        noteText.Text = transaction.Note ?? string.Empty;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (DialogResult != DialogResult.OK || e.Cancel)
        {
            return;
        }

        var amountInvalid = amountInput.Value <= 0;
        var categoryInvalid = string.IsNullOrWhiteSpace(categoryCombo.Text);
        errorProvider.SetError(amountInput, amountInvalid ? Strings.AmountMustBePositive : "");
        errorProvider.SetError(categoryCombo, categoryInvalid ? Strings.CategoryRequired : "");

        if (amountInvalid || categoryInvalid)
        {
            e.Cancel = true;
            DialogResult = DialogResult.None;
            (amountInvalid ? (Control)amountInput : categoryCombo).Focus();
            return;
        }

        Result = BuildTransaction();
    }

    private void amountInput_ValueChanged(object? sender, EventArgs e) =>
        errorProvider.SetError(amountInput, "");

    private void categoryCombo_TextChanged(object? sender, EventArgs e) =>
        errorProvider.SetError(categoryCombo, "");

    private Transaction BuildTransaction()
    {
        var transaction = Transaction.Create(
            DateOnly.FromDateTime(datePicker.Value),
            amountInput.Value,
            expenseRadio.Checked,
            categoryCombo.Text,
            noteText.Text);
        transaction.Id = editingId;
        return transaction;
    }
}
