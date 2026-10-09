using LocalExpense.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LocalExpense.Tests;

/// <summary>
/// A throwaway SQLite in-memory database with the real migrations applied.
/// An in-memory SQLite database lives only as long as its connection, so the connection
/// is opened once here and kept open; every context the factory creates shares it.
/// </summary>
internal sealed class TestDbFactory : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDbFactory()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        using var db = CreateDbContext();
        db.Database.Migrate();
    }

    public AppDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
