using System.Globalization;
using System.Text;
using LocalExpense.Cli;
using LocalExpense.Data;
using LocalExpense.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

const int MaxReportedErrors = 20;

const string Usage = """
    LocalExpense developer tool

    Usage:
      localexpense-cli import --input <file.csv> --database <path> [--create] [--dry-run]
      localexpense-cli seed   --output <file.csv> [--months 30] [--end yyyy-MM-dd] [--random-seed 42] [--force]

    import   Adds every row of a CSV (columns Date, Category, Amount, optional Note; an Id column is ignored).
             Nothing is written if any row is invalid. Importing a file twice adds its rows twice.
      -i, --input <file>      The CSV to read.
      -d, --database <path>   The SQLite database to write to. Required, so a typo can never hit the wrong file.
          --create            Create the database if it does not exist (otherwise a missing file is an error).
          --dry-run           Read and validate the file only; do not open the database.

    seed     Writes a sample CSV (about 30 months of income and expenses, with an empty month in the middle).
      -o, --output <file>     Where to write the CSV.
          --months <n>        Length of the span in months (default 30).
          --end <date>        Last day of the span (default today).
          --random-seed <n>   Same seed, same data (default 42).
          --force             Overwrite the output file if it exists.

      -h, --help              Show this text.
    """;

try
{
    return await RunAsync(args);
}
// Only failures the user can act on (a locked file, a file that is not a database, ...). A programming error keeps its stack trace.
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SqliteException or DbUpdateException)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    return 1;
}

async Task<int> RunAsync(string[] args)
{
    if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
    {
        Console.WriteLine(Usage);
        return args.Length == 0 ? 2 : 0;
    }

    var command = args[0];
    if (command is not ("import" or "seed"))
    {
        return Fail($"unknown command '{command}'.");
    }

    var options = ParseOptions(args.Skip(1).ToArray(), out var problem);
    if (problem is not null)
    {
        return Fail(problem);
    }

    if (options.ContainsKey("help"))
    {
        Console.WriteLine(Usage);
        return 0;
    }

    return command == "import" ? await ImportAsync(options) : Seed(options);
}

int Fail(string message)
{
    Console.Error.WriteLine($"error: {message}");
    Console.Error.WriteLine("Run with --help for usage.");
    return 2;
}

// --name value, --flag, and the short forms -i -d -o -h. Returns canonical long names.
Dictionary<string, string> ParseOptions(string[] tokens, out string? problem)
{
    var shorts = new Dictionary<string, string> { ["-i"] = "input", ["-d"] = "database", ["-o"] = "output", ["-h"] = "help" };
    var flags = new HashSet<string> { "create", "dry-run", "force", "help" };
    var valued = new HashSet<string> { "input", "database", "output", "months", "end", "random-seed" };
    var result = new Dictionary<string, string>();
    problem = null;
    for (var i = 0; i < tokens.Length; i++)
    {
        string? name = null;

        if (shorts.TryGetValue(tokens[i], out var longName))
        {
            name = longName;
        }
        else if (tokens[i].StartsWith("--"))
        {
            name = tokens[i][2..];
        }

        if (name is null || !(flags.Contains(name) || valued.Contains(name)))
        {
            problem = $"unknown option '{tokens[i]}'.";
            return result;
        }

        if (flags.Contains(name))
        {
            result[name] = "";
        }
        else if (i + 1 < tokens.Length && !string.IsNullOrWhiteSpace(tokens[i + 1]) && !tokens[i + 1].StartsWith('-'))
        {
            result[name] = tokens[++i];
        }
        else
        {
            problem = $"option '{tokens[i]}' needs a value.";
            return result;
        }
    }

    return result;
}

async Task<int> ImportAsync(Dictionary<string, string> options)
{
    if (!options.TryGetValue("input", out var input))
    {
        return Fail("--input is required.");
    }

    var dryRun = options.ContainsKey("dry-run");
    if (!dryRun && !options.ContainsKey("database"))
    {
        return Fail("--database is required.");
    }

    if (!File.Exists(input))
    {
        Console.Error.WriteLine($"error: input file not found: {Path.GetFullPath(input)}");
        return 1;
    }

    Console.WriteLine($"Reading {Path.GetFullPath(input)}");
    CsvImportResult result;
    using (var reader = new StreamReader(input, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
    {
        result = CsvImporter.Read(reader);
    }

    if (!result.Succeeded)
    {
        foreach (var error in result.Errors.Take(MaxReportedErrors))
        {
            Console.Error.WriteLine($"  line {error.Line}: {error.Message}");
        }

        if (result.Errors.Count > MaxReportedErrors)
        {
            Console.Error.WriteLine($"  ... and {result.Errors.Count - MaxReportedErrors} more");
        }

        Console.Error.WriteLine($"error: {result.Errors.Count} problem(s) found; nothing was imported.");
        return 1;
    }

    Console.WriteLine($"Read {result.Rows.Count} valid rows.");
    if (dryRun)
    {
        Console.WriteLine("Dry run: the database was not opened and nothing was written.");
        return 0;
    }

    var databasePath = Path.GetFullPath(options["database"]);
    var create = options.ContainsKey("create");
    if (!create && !File.Exists(databasePath))
    {
        Console.Error.WriteLine($"error: database not found: {databasePath} (use --create to make a new one)");
        return 1;
    }

    if (create)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
    }

    var mode = create ? "ReadWriteCreate" : "ReadWrite";
    var factory = new CliDbFactory(new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={databasePath};Mode={mode}").Options);
    using (var db = factory.CreateDbContext())
    {
        db.Database.Migrate();
    }

    Console.WriteLine($"Writing to {databasePath}");
    var added = await new TransactionService(factory).AddRangeAsync(result.Rows.ToList());
    Console.WriteLine($"Imported {added} transactions.");
    return 0;
}

int Seed(Dictionary<string, string> options)
{
    if (!options.TryGetValue("output", out var output))
    {
        return Fail("--output is required.");
    }

    if (!TryInt(options, "months", 30, out var months) || months is < 1 or > 600)
    {
        return Fail("--months must be a whole number from 1 to 600.");
    }

    if (!TryInt(options, "random-seed", 42, out var randomSeed))
    {
        return Fail("--random-seed must be a whole number.");
    }

    var end = DateOnly.FromDateTime(DateTime.Today);
    if (options.TryGetValue("end", out var endText)
        && !DateOnly.TryParseExact(endText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
    {
        return Fail("--end must be a date like 2026-10-09.");
    }

    var path = Path.GetFullPath(output);
    if (File.Exists(path) && !options.ContainsKey("force"))
    {
        Console.Error.WriteLine($"error: {path} already exists (use --force to overwrite)");
        return 1;
    }

    var rows = SeedGenerator.Generate(end, months, randomSeed);
    using (var writer = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
    {
        CsvExporter.Write(rows, writer);
    }

    Console.WriteLine($"Wrote {rows.Count} transactions ({rows[^1].Date:yyyy-MM-dd} to {rows[0].Date:yyyy-MM-dd}) to {path}");
    return 0;
}

static bool TryInt(Dictionary<string, string> options, string name, int fallback, out int value)
{
    value = fallback;
    return !options.TryGetValue(name, out var text) || int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
}

internal sealed class CliDbFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() => new(options);
}
