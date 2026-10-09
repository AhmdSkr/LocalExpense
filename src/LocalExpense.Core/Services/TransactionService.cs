using LocalExpense.Data;
using LocalExpense.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalExpense.Services;

public class TransactionService(IDbContextFactory<AppDbContext> factory)
{
    public const int MaxCategoryLength = 100;
    public const int MaxNoteLength = 500;

    public async Task<List<Transaction>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Transactions.AsNoTracking()
            .OrderByDescending(t => t.Date).ThenByDescending(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<Transaction?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    /// <summary>Inclusive on both ends.</summary>
    public async Task<List<Transaction>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Transactions.AsNoTracking()
            .Where(t => t.Date >= from && t.Date <= to)
            .OrderByDescending(t => t.Date).ThenByDescending(t => t.Id)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Inclusive on both ends; a null bound means no limit on that side. A null <paramref name="category"/>
    /// means every category; otherwise it must match exactly, the same way <see cref="GetCategoriesAsync"/> lists them.
    /// </summary>
    public async Task<List<Transaction>> GetFilteredAsync(DateOnly? from, DateOnly? to, string? category = null, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var query = db.Transactions.AsNoTracking();
        if (from is { } start)
            query = query.Where(t => t.Date >= start);
        if (to is { } end)
            query = query.Where(t => t.Date <= end);
        if (category is not null)
            query = query.Where(t => t.Category == category);
        return await query
            .OrderByDescending(t => t.Date).ThenByDescending(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken ct = default)
    {
        Validate(transaction);
        await using var db = await factory.CreateDbContextAsync(ct);
        transaction.Id = 0;
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(ct);
        return transaction;
    }

    /// <returns>false if no transaction with that Id exists.</returns>
    public async Task<bool> UpdateAsync(Transaction transaction, CancellationToken ct = default)
    {
        Validate(transaction);
        await using var db = await factory.CreateDbContextAsync(ct);
        var rows = await db.Transactions.Where(t => t.Id == transaction.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Date, transaction.Date)
                .SetProperty(t => t.AmountMinor, transaction.AmountMinor)
                .SetProperty(t => t.Category, transaction.Category)
                .SetProperty(t => t.Note, transaction.Note), ct);
        return rows > 0;
    }

    /// <returns>false if no transaction with that Id exists.</returns>
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Transactions.Where(t => t.Id == id).ExecuteDeleteAsync(ct) > 0;
    }

    /// <summary>Deletes every transaction whose Id is listed, in one statement. Unknown Ids are ignored.</summary>
    /// <returns>How many rows were actually deleted.</returns>
    public async Task<int> DeleteManyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return 0;
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Transactions.Where(t => ids.Contains(t.Id)).ExecuteDeleteAsync(ct);
    }

    /// <summary>Net total in minor units (income minus expenses). Inclusive on both ends.</summary>
    public async Task<long> GetTotalAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Transactions
            .Where(t => t.Date >= from && t.Date <= to)
            .SumAsync(t => t.AmountMinor, ct);
    }

    /// <summary>Net total per category in minor units. Inclusive on both ends.</summary>
    public async Task<Dictionary<string, long>> GetTotalsByCategoryAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Transactions
            .Where(t => t.Date >= from && t.Date <= to)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(t => t.AmountMinor) })
            .ToDictionaryAsync(x => x.Category, x => x.Total, ct);
    }

    public async Task<List<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Transactions.Select(t => t.Category).Distinct().OrderBy(c => c).ToListAsync(ct);
    }

    private static void Validate(Transaction t)
    {
        if (t.AmountMinor == 0)
            throw new ArgumentException("Amount cannot be zero.", nameof(t));
        if (string.IsNullOrWhiteSpace(t.Category))
            throw new ArgumentException("Category is required.", nameof(t));
        t.Category = t.Category.Trim();
        if (t.Category.Length > MaxCategoryLength)
            throw new ArgumentException($"Category cannot exceed {MaxCategoryLength} characters.", nameof(t));
        if (t.Note is { Length: > MaxNoteLength })
            throw new ArgumentException($"Note cannot exceed {MaxNoteLength} characters.", nameof(t));
    }
}
