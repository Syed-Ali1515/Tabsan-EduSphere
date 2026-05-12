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
    if (!existingColumns.Contains("IncludeSchool"))
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "ALTER TABLE issued_keys ADD COLUMN IncludeSchool INTEGER NOT NULL DEFAULT 0;";
        await cmd.ExecuteNonQueryAsync();
    }
    if (!existingColumns.Contains("IncludeCollege"))
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "ALTER TABLE issued_keys ADD COLUMN IncludeCollege INTEGER NOT NULL DEFAULT 0;";
        await cmd.ExecuteNonQueryAsync();
    }
    if (!existingColumns.Contains("IncludeUniversity"))
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "ALTER TABLE issued_keys ADD COLUMN IncludeUniversity INTEGER NOT NULL DEFAULT 1;";
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

Console.Clear();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║      Tabsan EduSphere Vendor Licensing Tool         ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine();
Console.WriteLine("Private signing keys must remain only on vendor-controlled systems.");
Console.WriteLine("This wizard creates a signed .tablic license file for EduSphere import.");
Console.WriteLine();

using (var runScope = sp.CreateScope())
{
    var keySvc = runScope.ServiceProvider.GetRequiredService<KeyService>();
    var licBuilder = runScope.ServiceProvider.GetRequiredService<LicenseBuilder>();
    await HandleGenerateLicenseFile(keySvc, licBuilder);
}

Console.WriteLine();
Console.WriteLine("Done.");

// ── Handlers ─────────────────────────────────────────────────────────────────

static async Task HandleGenerateLicenseFile(KeyService keySvc, LicenseBuilder builder)
{
    Console.WriteLine("Create signed license (.tablic)");
    Console.WriteLine();

    var expiry = PromptExpiry();
    if (expiry is null)
    {
        WriteError("Invalid expiry selection.");
        return;
    }

    Console.Write("  Customer/Tenant label (optional): ");
    var label = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(label))
        label = null;

    Console.Write("  Max concurrent users (0 = unlimited): ");
    var maxUsersInput = Console.ReadLine()?.Trim();
    if (!int.TryParse(maxUsersInput, out var maxUsers) || maxUsers < 0)
    {
        WriteError("Invalid MaxUsers. Must be a non-negative integer (0 = unlimited).");
        return;
    }

    Console.Write("  Allowed domain (optional, blank = unrestricted): ");
    var domainInput = Console.ReadLine()?.Trim();
    var allowedDomain = string.IsNullOrWhiteSpace(domainInput) ? null : domainInput.ToLowerInvariant();

    var scope = PromptInstitutionScope();
    if (scope is null)
        return;

    var (record, _) = await keySvc.GenerateAsync(expiry.Value, label);
    record.MaxUsers = maxUsers;
    record.AllowedDomain = allowedDomain;
    record.IncludeSchool = scope.Value.IncludeSchool;
    record.IncludeCollege = scope.Value.IncludeCollege;
    record.IncludeUniversity = scope.Value.IncludeUniversity;
    await keySvc.UpdateConstraintsAsync(record);

    var licenseFolder = Path.Combine(AppContext.BaseDirectory, "license");
    var fileName = $"tabsan-license-{record.KeyId:N}.tablic";
    var outPath = Path.Combine(licenseFolder, fileName);

    try
    {
        Directory.CreateDirectory(licenseFolder);

        await builder.BuildAsync(record, outPath);
        var verificationKey = LicenseBuilder.ComputeVerificationKey();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine();
        Console.WriteLine("  ✓ License file generated successfully.");
        Console.ResetColor();
        Console.WriteLine($"  File       : {outPath}");
        Console.WriteLine($"  KeyId      : {record.KeyId}");
        Console.WriteLine($"  VerificationKey : {verificationKey}");
        Console.WriteLine($"  Expires    : {FormatExpiry(record.ExpiryType, record.ExpiresAt)}");
        Console.WriteLine($"  Scope      : {FormatInstitutionScope(record.IncludeSchool, record.IncludeCollege, record.IncludeUniversity)}");
        Console.WriteLine($"  MaxUsers   : {(record.MaxUsers == 0 ? "Unlimited" : record.MaxUsers.ToString())}");
        Console.WriteLine($"  Domain     : {record.AllowedDomain ?? "Any"}");
        Console.WriteLine();
        Console.WriteLine("Import this .tablic file in EduSphere: Portal -> Settings -> License Update.");
    }
    catch (Exception ex)
    {
        WriteError($"Failed to build .tablic: {ex.Message}");
    }
}

// ── Utilities ─────────────────────────────────────────────────────────────────

static ExpiryType? PromptExpiry()
{
    Console.WriteLine("  Expiry type:");
    Console.WriteLine("    [1] 1 month");
    Console.WriteLine("    [2] 1 year");
    Console.WriteLine("    [3] 2 years");
    Console.WriteLine("    [4] 3 years");
    Console.WriteLine("    [5] Permanent");
    Console.Write("  Choice: ");
    return Console.ReadLine()?.Trim() switch
    {
        "1" => ExpiryType.OneMonth,
        "2" => ExpiryType.OneYear,
        "3" => ExpiryType.TwoYears,
        "4" => ExpiryType.ThreeYears,
        "5" => ExpiryType.Permanent,
        _   => null
    };
}

static (bool IncludeSchool, bool IncludeCollege, bool IncludeUniversity)? PromptInstitutionScope()
{
    Console.WriteLine("  Institution scope (enter y/n for each type; at least one must be enabled):");
    var includeSchool = PromptYesNo("    Include School? (y/n): ");
    var includeCollege = PromptYesNo("    Include College? (y/n): ");
    var includeUniversity = PromptYesNo("    Include University? (y/n): ");

    if (!includeSchool && !includeCollege && !includeUniversity)
    {
        WriteError("At least one institution type must be enabled.");
        return null;
    }

    return (includeSchool, includeCollege, includeUniversity);
}

static bool PromptYesNo(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var raw = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (raw is "y" or "yes") return true;
        if (raw is "n" or "no") return false;
        WriteError("Enter y or n.");
    }
}

static string FormatExpiry(ExpiryType expiryType, DateTime? expiresAt)
    => expiryType switch
    {
        ExpiryType.OneMonth   => expiresAt?.ToString("yyyy-MM-dd") is { } date ? $"1 month ({date})" : "1 month",
        ExpiryType.OneYear    => expiresAt?.ToString("yyyy-MM-dd") is { } date ? $"1 year ({date})" : "1 year",
        ExpiryType.TwoYears   => expiresAt?.ToString("yyyy-MM-dd") is { } date ? $"2 years ({date})" : "2 years",
        ExpiryType.ThreeYears => expiresAt?.ToString("yyyy-MM-dd") is { } date ? $"3 years ({date})" : "3 years",
        ExpiryType.Permanent  => "Permanent",
        _                     => expiresAt?.ToString("yyyy-MM-dd") ?? expiryType.ToString()
    };

static string FormatInstitutionScope(bool includeSchool, bool includeCollege, bool includeUniversity)
{
    var enabled = new List<string>(3);
    if (includeSchool) enabled.Add("School");
    if (includeCollege) enabled.Add("College");
    if (includeUniversity) enabled.Add("University");
    return enabled.Count == 0 ? "None" : string.Join(", ", enabled);
}

static void WriteError(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  ✗ {msg}");
    Console.ResetColor();
}

