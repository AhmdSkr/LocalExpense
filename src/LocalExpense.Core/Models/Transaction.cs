namespace LocalExpense.Models;

 public class Transaction
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }

    /// <summary>Signed amount in minor units (see <see cref="Money"/>). Negative = expense, positive = income.</summary>
    public long AmountMinor { get; set; }

    public string Category { get; set; } = string.Empty;
    public string? Note { get; set; }

    /// <summary>
    /// Builds a transaction from user input: a positive major-unit <paramref name="amount"/> plus a direction,
    /// trimmed text, and a blank note stored as null. Validation of the result stays in TransactionService.
    /// </summary>
    public static Transaction Create(DateOnly date, decimal amount, bool isExpense, string category, string? note)
    {
        var minor = Money.ToMinor(amount);
        return new Transaction
        {
            Date = date,
            AmountMinor = isExpense ? -minor : minor,
            Category = category.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
        };
    }
}
