using LocalExpense.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LocalExpense
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var builder = Host.CreateApplicationBuilder();

            // OnConfiguring is skipped when DI supplies options, so create the folder here.
            Directory.CreateDirectory(Path.GetDirectoryName(AppDbContext.DbPath)!);
            builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseSqlite($"Data Source={AppDbContext.DbPath}"));
            builder.Services.AddSingleton<Services.TransactionService>();
            builder.Services.AddTransient<Form1>();

            using var host = builder.Build();

            using (var db = host.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
            {
                db.Database.Migrate();
            }

            ApplicationConfiguration.Initialize();
            Application.Run(host.Services.GetRequiredService<Form1>());
        }
    }
}
