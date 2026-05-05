using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tabsan.Lic.Data;
using Tabsan.Lic.Models;
using Tabsan.Lic.Services;

// ── Bootstrap ────────────────────────────────────────────────────────────────
var services = new ServiceCollection();

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Tabsan", "tabsan_lic.db");

Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

services.AddDbContext<LicDb>(o => o.UseSqlite($"Data Source={dbPath}"));
services.AddScoped<KeyService>();
services.AddScoped<LicenseBuilder>();

var sp = services.BuildServiceProvider();

// Ensure DB is created / migrated
using (var scope = sp.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LicDb>();
    await db.Database.EnsureCreatedAsync();

    // P3-S1-01: Add Phase 2 columns to existing databases that pre-date this update.
    // SQLite does not support IF NOT EXISTS in ALTER TABLE, so we inspect PRAGMA first.
    var conn = db.Database.GetDbConnection();
    await conn.OpenAsync();
    using var pragmaCmd = conn.CreateCommand();
    pragmaCmd.CommandText = "PRAGMA table_info(issued_keys);";
    var existingColumns = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
    using (var reader = await pragmaCmd.ExecuteReaderAsync())
        while (await reader.ReadAsync())
            existingColumns.Add(reader.GetString(1)); // column 1 = name

    if (!existingColumns.Contains("MaxUsers"))
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "ALTER TABLE issued_keys ADD COLUMN MaxUsers INTEGER NOT NULL DEFAULT 0;";
        await cmd.ExecuteNonQueryAsync();
    }
    if (!existingColumns.Contains("AllowedDomain"))
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "ALTER TABLE issued_keys ADD COLUMN AllowedDomain TEXT NULL;";
        await cmd.ExecuteNonQueryAsync();
    }
    await conn.CloseAsync();
}

// ── Main Menu ─────────────────────────────────────────────────────────────────
bool running = true;
while (running)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔══════════════════════════════════════════╗");
    Console.WriteLine("║         Tabsan-Lic  v1.0  License Tool  ║");
    Console.WriteLine("╚══════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("  [1] Generate single VerificationKey");
    Console.WriteLine("  [2] Generate bulk VerificationKeys");
    Console.WriteLine("  [3] Generate .tablic file for a key");
    Console.WriteLine("  [4] List all issued keys");
    Console.WriteLine("  [5] Export key list to CSV");
    Console.WriteLine("  [0] Exit");
    Console.WriteLine();
    Console.Write("  Choice: ");

    var choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    using var menuScope = sp.CreateScope();
    var keySvc     = menuScope.ServiceProvider.GetRequiredService<KeyService>();
    var licBuilder = menuScope.ServiceProvider.GetRequiredService<LicenseBuilder>();

    switch (choice)
    {
        case "1":
            await HandleGenerateSingle(keySvc);
            break;
        case "2":
            await HandleGenerateBulk(keySvc);
            break;
        case "3":
            await HandleBuildTablic(keySvc, licBuilder);
            break;
        case "4":
            await HandleListKeys(keySvc);
            break;
        case "5":
            await HandleExportCsv(keySvc);
            break;
        case "0":
            running = false;
            break;
        default:
            WriteError("Unknown option. Press Enter to continue.");
            Console.ReadLine();
            break;
    }
}

Console.WriteLine("Goodbye.");

// ── Handlers ─────────────────────────────────────────────────────────────────

static async Task HandleGenerateSingle(KeyService keySvc)
{
    var expiry = PromptExpiry();
    if (expiry is null) return;

    Console.Write("  Label (optional): ");
    var label = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(label)) label = null;

    var (record, token) = await keySvc.GenerateAsync(expiry.Value, label);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine();
    Console.WriteLine($"  ✓ Key generated. Id={record.Id}  KeyId={record.KeyId}");
    Console.WriteLine($"  VerificationKey (show once — record securely):");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"    {token}");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine($"  Hash (stored in DB): {record.VerificationKeyHash}");
    Console.WriteLine($"  Expiry: {record.ExpiresAt?.ToString("yyyy-MM-dd") ?? "Permanent"}");

    Pause();
}

static async Task HandleGenerateBulk(KeyService keySvc)
{
    var expiry = PromptExpiry();
    if (expiry is null) return;

    Console.Write("  Number of keys to generate: ");
    if (!int.TryParse(Console.ReadLine(), out var count) || count < 1 || count > 1000)
    {
        WriteError("Invalid count (1–1000).");
        Pause();
        return;
    }

    Console.Write("  Label prefix (optional): ");
    var prefix = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(prefix)) prefix = null;

    var results = await keySvc.GenerateBulkAsync(count, expiry.Value, prefix);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n  ✓ {results.Count} keys generated.\n");
    Console.ResetColor();
    Console.WriteLine("  Id  | KeyId (short)       | Token");
    Console.WriteLine("  ----|---------------------|-----------------------------------");
    foreach (var (rec, tok) in results)
        Console.WriteLine($"  {rec.Id,-4}| {rec.KeyId.ToString()[..8],-21}| {tok}");

    Pause();
}

static async Task HandleBuildTablic(KeyService keySvc, LicenseBuilder builder)
{
    Console.Write("  Enter key Id to generate .tablic for: ");
    if (!int.TryParse(Console.ReadLine(), out var id))
    {
        WriteError("Invalid Id."); Pause(); return;
    }

    var key = await keySvc.GetByIdAsync(id);
    if (key is null) { WriteError("Key not found."); Pause(); return; }

    if (key.IsLicenseGenerated)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Warning: a .tablic was already generated for this key on {key.LicenseGeneratedAt:O}.");
        Console.WriteLine("  Generating a second file is allowed but the VerificationKey can only be used ONCE in EduSphere.");
        Console.ResetColor();
        Console.Write("  Continue? (y/n): ");
        if (Console.ReadLine()?.Trim().ToLower() != "y") return;
    }

    // P3-S1-01: Prompt for Phase 2 constraint fields
    Console.WriteLine();
    Console.WriteLine("  ── License Constraints (Phase 2) ──────────────────────────────────");
    Console.Write("  Max concurrent users (0 = unlimited): ");
    var maxUsersInput = Console.ReadLine()?.Trim();
    if (!int.TryParse(maxUsersInput, out var maxUsers) || maxUsers < 0)
    {
        WriteError("Invalid MaxUsers. Must be a non-negative integer (0 = unlimited)."); Pause(); return;
    }
    key.MaxUsers = maxUsers;

    Console.Write("  Allowed domain (leave blank for no restriction, e.g. portal.university.edu): ");
    var domainInput = Console.ReadLine()?.Trim();
    key.AllowedDomain = string.IsNullOrWhiteSpace(domainInput) ? null : domainInput.ToLowerInvariant();

    await keySvc.UpdateConstraintsAsync(key);

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"  MaxUsers    : {(maxUsers == 0 ? "Unlimited" : maxUsers.ToString())}");
    Console.WriteLine($"  AllowedDomain: {key.AllowedDomain ?? "(none — unrestricted)"}");
    Console.ResetColor();
    Console.WriteLine();

    Console.Write("  Output path (e.g. C:\\Licenses\\license.tablic): ");
    var outPath = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(outPath)) { WriteError("Invalid path."); Pause(); return; }

    if (!outPath.EndsWith(".tablic", StringComparison.OrdinalIgnoreCase))
        outPath += ".tablic";

    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        await builder.BuildAsync(key, outPath);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  ✓ .tablic file written to: {outPath}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        WriteError($"Failed to build .tablic: {ex.Message}");
    }

    Pause();
}

static async Task HandleListKeys(KeyService keySvc)
{
    var keys = await keySvc.ListAllAsync();
    if (keys.Count == 0) { Console.WriteLine("  No keys issued yet."); Pause(); return; }

    // P3-S1-01: Show MaxUsers and AllowedDomain in list
    Console.WriteLine($"  {"Id",-4} {"KeyId (short)",-10} {"Expiry",-12} {"MaxUsers",-10} {"LicGenerated",-14} {"AllowedDomain",-30} Label");
    Console.WriteLine(new string('-', 110));
    foreach (var k in keys)
    {
        var expiry  = k.ExpiresAt?.ToString("yyyy-MM-dd") ?? "Permanent";
        var licMark = k.IsLicenseGenerated ? "Yes" : "No";
        var maxU    = k.MaxUsers == 0 ? "Unlimited" : k.MaxUsers.ToString();
        var domain  = k.AllowedDomain ?? "(any)";
        Console.WriteLine($"  {k.Id,-4} {k.KeyId.ToString()[..8],-10} {expiry,-12} {maxU,-10} {licMark,-14} {domain,-30} {k.Label}");
    }

    Pause();
}

static async Task HandleExportCsv(KeyService keySvc)
{
    var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    var path    = Path.Combine(desktop, $"tabsan-keys-{DateTime.UtcNow:yyyyMMddHHmm}.csv");

    // P3-S1-01: CSV now includes MaxUsers and AllowedDomain columns
    var csv = await keySvc.ExportCsvAsync();
    await File.WriteAllTextAsync(path, csv, System.Text.Encoding.UTF8);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n  ✓ CSV exported to: {path}");
    Console.ResetColor();

    Pause();
}

// ── Utilities ─────────────────────────────────────────────────────────────────

static ExpiryType? PromptExpiry()
{
    Console.WriteLine("  Expiry type:");
    Console.WriteLine("    [1] 1 year");
    Console.WriteLine("    [2] 2 years");
    Console.WriteLine("    [3] 3 years");
    Console.WriteLine("    [4] Permanent");
    Console.Write("  Choice: ");
    return Console.ReadLine()?.Trim() switch
    {
        "1" => ExpiryType.OneYear,
        "2" => ExpiryType.TwoYears,
        "3" => ExpiryType.ThreeYears,
        "4" => ExpiryType.Permanent,
        _   => null
    };
}

static void WriteError(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  ✗ {msg}");
    Console.ResetColor();
}

static void Pause()
{
    Console.WriteLine();
    Console.Write("  Press Enter to return to menu…");
    Console.ReadLine();
}
