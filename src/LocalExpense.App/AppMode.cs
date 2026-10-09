namespace LocalExpense;

/// <summary>
/// How this process was started. <see cref="IsDemo"/> is set by the --demo switch: the window then runs on a throwaway
/// database filled with the bundled sample data, never on the user's own.
/// </summary>
public sealed record AppMode(bool IsDemo);
