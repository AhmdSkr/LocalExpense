using LocalExpense.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LocalExpense.Tests;

/// <summary>
/// A throwaway SQLite database in a temp file with the real migrations applied.
/// Unlike <see cref="TestDbFactory"/>, every context opens its own connection, as the app does,
/// which is what concurrency tests need (one SqliteConnection must not be used from several threads).
/// </summary>
internal sealed class TestFileDbFactory : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"localexpense-test-{Guid.NewGuid():N}.db");
    private readonly DbContextOptions<AppDbContext> _options;

    public TestFileDbFactory()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={_path}").Options;

        using var db = CreateDbContext();
        db.Database.Migrate();
    }

    public AppDbContext CreateDbContext() => new(_options);

    public void Dispose()
    {
        // Pooled connections keep the file locked; release them before deleting.
        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _path, _path + "-wal", _path + "-shm", _path + "-journal" })
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }
}
