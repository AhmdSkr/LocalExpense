namespace LocalExpense;

internal static class FormHelpers
{
    /// <summary>Runs an operation started from the UI and shows any failure in a message box owned by <paramref name="form"/>.</summary>
    public static async Task TryAsync(this Form form, Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            MessageBox.Show(form, ex.Message, form.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// The colour of a signed amount wherever one is shown: red for expenses, green for income, grey for nothing.
    /// The sign is always shown as well, so colour is never the only cue.
    /// </summary>
    public static Color AmountColor(long minor) =>
        minor < 0 ? Color.Firebrick : minor > 0 ? Color.SeaGreen : SystemColors.GrayText;
}

/// <summary>
/// Runs a window's data query so that starting a new one cancels the one in flight, and only the newest result is delivered.
/// Lets the filters and period combos change while a query runs without ever showing an older result over a newer one.
/// </summary>
internal sealed class LatestQuery
{
    private CancellationTokenSource? current;

    /// <returns>The result, or null when a newer query or <see cref="Cancel"/> superseded this one.</returns>
    public async Task<T?> RunAsync<T>(Func<CancellationToken, Task<T>> query) where T : class
    {
        Cancel();
        var cts = current = new CancellationTokenSource();
        try
        {
            var result = await query(cts.Token);

            // The query can finish in the same instant it is cancelled, so check again before handing the result over.
            return cts.IsCancellationRequested ? null : result;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            return null;
        }
    }

    public void Cancel()
    {
        current?.Cancel();
        current?.Dispose();
        current = null;
    }
}
