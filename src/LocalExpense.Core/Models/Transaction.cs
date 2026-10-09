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
    /// normalized as <see cref="Normalize"/> describes. Validation of the result stays in TransactionService.
    /// </summary>
    public static Transaction Create(DateOnly date, decimal amount, bool isExpense, string category, string? note)
    {
        var minor = Money.ToMinor(amount);
        var transaction = new Transaction
        {
            Date = date,
            AmountMinor = isExpense ? -minor : minor,
            Category = category,
            Note = note,
        };
        transaction.Normalize();
        return transaction;
    }

    /// <summary>Trims Category and Note and stores a blank Note as null, in place. Every path that saves a transaction runs this first.</summary>
    public void Normalize()
    {
        Category = Category?.Trim() ?? string.Empty;
        Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim();
    }
}
