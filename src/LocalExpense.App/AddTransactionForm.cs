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

        if (amountInput.Value <= 0)
        {
            Reject("Enter an amount greater than zero.", amountInput, e);
        }
        else if (string.IsNullOrWhiteSpace(categoryCombo.Text))
        {
            Reject("Category is required.", categoryCombo, e);
        }
        else
        {
            Result = BuildTransaction();
        }
    }

    private Transaction BuildTransaction() => Transaction.Create(
        DateOnly.FromDateTime(datePicker.Value),
        amountInput.Value,
        expenseRadio.Checked,
        categoryCombo.Text,
        noteText.Text);

    private void Reject(string message, Control field, FormClosingEventArgs e)
    {
        e.Cancel = true;
        DialogResult = DialogResult.None;
        MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        field.Focus();
    }
}
