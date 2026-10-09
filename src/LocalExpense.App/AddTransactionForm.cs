using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense;

/// <summary>
/// Modal dialog that collects the fields of a new transaction. It never touches the database:
/// on <see cref="DialogResult.OK"/> the caller reads <see cref="Result"/> and saves it.
/// </summary>
public partial class AddTransactionForm : Form
{
    // Keeps Money.ToMinor's checked conversion far from overflowing a long.
    private const decimal MaxAmount = 999_999_999m;

    public AddTransactionForm()
    {
        InitializeComponent();
    }

    // Runtime-only setup lives here, not in the constructor, so the Designer never sees it
    // and cannot serialize it into InitializeComponent.
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        amountInput.DecimalPlaces = Money.Exponent;
        amountInput.Maximum = MaxAmount;
        categoryCombo.MaxLength = TransactionService.MaxCategoryLength;
        noteText.MaxLength = TransactionService.MaxNoteLength;
        datePicker.Value = DateTime.Today;
    }

    /// <summary>The validated transaction; null unless the dialog was closed with OK.</summary>
    public Transaction? Result { get; private set; }

    public void SetCategories(IEnumerable<string> categories)
    {
        categoryCombo.Items.Clear();
        categoryCombo.Items.AddRange(categories.ToArray<object>());
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
        errorProvider.SetError(amountInput, amountInvalid ? "Enter an amount greater than zero." : "");
        errorProvider.SetError(categoryCombo, categoryInvalid ? "Category is required." : "");

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

    private Transaction BuildTransaction() => Transaction.Create(
        DateOnly.FromDateTime(datePicker.Value),
        amountInput.Value,
        expenseRadio.Checked,
        categoryCombo.Text,
        noteText.Text);
}
