namespace LocalExpense.Models;

 public class Transaction
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }

    /// <summary>Signed amount in minor units (see <see cref="Money"/>). Negative = expense, positive = income.</summary>
    public long AmountMinor { get; set; }

    public string Category { get; set; } = string.Empty;
    public string? Note { get; set; }
}
