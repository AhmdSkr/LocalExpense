# LocalExpense

A local-first personal expense tracker for Windows. Your data stays in a single SQLite file on your machine: no server, no account, no network.

Built with .NET 10, Windows Forms and Entity Framework Core (SQLite).

> **Status:** the data layer and `TransactionService` are done and tested; the UI is not built yet.

## Structure

```
src/LocalExpense.App     WinForms front end and host/DI setup
src/LocalExpense.Core    Models, Money, EF Core context + migrations, TransactionService
tests/LocalExpense.Tests xUnit tests (real SQLite, including concurrency)
```

## Getting started

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) on Windows.

```bash
dotnet test LocalExpense.sln
dotnet run --project src/LocalExpense.App
```

The database is created and migrated on first launch at `%LocalAppData%\LocalExpense\localexpense.db`.

## Design notes

- **Money is stored as integer cents** (`long AmountMinor`), never as decimal, so totals are exact and summed in SQL. Currency and exponent are hard-coded in `Money.cs` (USD, 2 decimals). Change them before storing real data.
- **Negative amounts are expenses, positive amounts are income.**
- Each service call opens a short-lived `DbContext` from a factory; no context is shared across the UI.

## Migrations

```bash
dotnet ef migrations add <Name> -o Data/Migrations --project src/LocalExpense.Core --startup-project src/LocalExpense.App
```

## License

See [LICENSE.txt](LICENSE.txt).
