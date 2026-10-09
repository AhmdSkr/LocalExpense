using LocalExpense.Data;
using LocalExpense.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocalExpense
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application. With --demo, the window runs on a throwaway database
        ///  filled with the bundled sample data, which is deleted again when the window closes.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var demo = args.Contains("--demo");
            var dbPath = demo
                ? Path.Combine(Path.GetTempPath(), $"LocalExpense-demo-{Guid.NewGuid():N}.db")
                : AppDbContext.DbPath;

            // Not given args: the command-line configuration provider would reject a bare switch like --demo.
            var builder = Host.CreateApplicationBuilder();

            // The host's defaults also log to the console and the Windows Application event log, which would put a desktop app's
            // routine warnings (such as EF Core's on every new demo database) in the user's event log. Keep only the debugger output.
            builder.Logging.ClearProviders();
            builder.Logging.AddDebug();

            // OnConfiguring is skipped when DI supplies options, so create the folder here.
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));
            builder.Services.AddSingleton<TransactionService>();
            builder.Services.AddSingleton(new AppMode(demo));
            builder.Services.AddTransient<MainForm>();
            builder.Services.AddTransient<ReportsForm>();

            try
            {
                using var host = builder.Build();

                using (var db = host.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
                {
                    db.Database.Migrate();
                }

                if (demo)
                {
                    LoadSampleData(host.Services.GetRequiredService<TransactionService>());
                }

                ApplicationConfiguration.Initialize();
                Application.Run(host.Services.GetRequiredService<MainForm>());
            }
            finally
            {
                if (demo)
                {
                    DeleteDemoDatabase(dbPath);
                }
            }
        }

        // The Khoury family's three years (samples/lebanon-family-expenses.csv, embedded in the exe), moved so they end last month.
        private static void LoadSampleData(TransactionService service)
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream("LocalExpense.SampleData.csv")!;
            using var reader = new StreamReader(stream);
            var result = CsvImporter.Read(reader);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"The bundled sample data is invalid: line {result.Errors[0].Line}: {result.Errors[0].Message}");
            }

            var rows = result.Rows.ToList();
            SampleData.ShiftToEndLastMonth(rows, DateOnly.FromDateTime(DateTime.Today));
            service.AddRangeAsync(rows).GetAwaiter().GetResult();
        }

        // Best effort: pooled connections hold the file open, so release them first. A file left behind is only a temp file.
        private static void DeleteDemoDatabase(string path)
        {
            SqliteConnection.ClearAllPools();
            try
            {
                File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }
    }
}
