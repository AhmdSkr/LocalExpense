using LocalExpense.Models;
using LocalExpense.Services;
using Microsoft.EntityFrameworkCore;

namespace LocalExpense.Data;

public class AppDbContext : DbContext
{
    public static string DbPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LocalExpense", "localexpense.db");

    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (options.IsConfigured)
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(t => t.Category).IsRequired().HasMaxLength(TransactionService.MaxCategoryLength);
            e.Property(t => t.Note).HasMaxLength(TransactionService.MaxNoteLength);
            e.HasIndex(t => t.Date);
        });
    }
}
