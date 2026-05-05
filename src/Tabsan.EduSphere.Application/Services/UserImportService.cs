using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Parses a CSV stream and bulk-creates user accounts (P4-S1-01).
/// Rules:
///   - CSV header row required: Username,Email,FullName,Role (DepartmentId optional 5th column)
///   - Initial password = Username (P4-S2-01)
///   - MustChangePassword is set to true so the user is forced to change on first login (P4-S2-02)
///   - Rows with duplicate usernames (in batch or existing in DB) are counted as duplicates
///   - Rows with missing/invalid fields are counted as errors and skipped
///   - Valid rows are inserted as a single batch after all rows are parsed
/// </summary>
public class UserImportService : IUserImportService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _hasher;

    /// <summary>
    /// Allowed role names for CSV import. SuperAdmin cannot be created via CSV
    /// (must be provisioned by the super-admin directly).
    /// </summary>
    private static readonly HashSet<string> AllowedRoles =
        new(StringComparer.OrdinalIgnoreCase) { "Admin", "Faculty", "Student" };

    public UserImportService(IUserRepository userRepo, IPasswordHasher hasher)
    {
        _userRepo = userRepo;
        _hasher = hasher;
    }

    public async Task<UserImportResult> ImportFromCsvAsync(Stream csvStream, CancellationToken ct = default)
    {
        var errors = new List<string>();
        var toImport = new List<User>();
        // Track usernames in the current batch to detect intra-batch duplicates.
        var batchUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Cache resolved role IDs to avoid a DB round-trip per row.
        var roleCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int totalRows = 0, duplicates = 0;

        using var reader = new StreamReader(csvStream);

        // Skip header line
        var header = await reader.ReadLineAsync(ct);
        if (header is null)
            return new UserImportResult(0, 0, 0, 0, new List<string>());

        // Validate that the header contains at least the 4 required columns
        var headerParts = header.Split(',');
        if (headerParts.Length < 4)
        {
            errors.Add("Invalid CSV format: header must have at least 4 columns (Username,Email,FullName,Role).");
            return new UserImportResult(0, 0, 0, 1, errors);
        }

        int lineNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            lineNumber++;
            totalRows++;

            if (string.IsNullOrWhiteSpace(line))
            {
                totalRows--; // blank lines don't count
                continue;
            }

            var parts = line.Split(',');
            if (parts.Length < 4)
            {
                errors.Add($"Line {lineNumber}: Expected at least 4 columns. Got {parts.Length}.");
                continue;
            }

            var username     = parts[0].Trim();
            var email        = parts[1].Trim();
            var roleName     = parts[3].Trim();
            var deptIdStr    = parts.Length >= 5 ? parts[4].Trim() : string.Empty;

            // ── Validate username ─────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(username))
            {
                errors.Add($"Line {lineNumber}: Username is empty.");
                continue;
            }

            // ── Validate role ─────────────────────────────────────────────────
            if (!AllowedRoles.Contains(roleName))
            {
                errors.Add($"Line {lineNumber}: Role '{roleName}' is not allowed. Must be one of: Admin, Faculty, Student.");
                continue;
            }

            // ── Validate email (optional, but must be valid if provided) ──────
            string? emailValue = null;
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!email.Contains('@') || email.Length > 256)
                {
                    errors.Add($"Line {lineNumber}: Email '{email}' is invalid.");
                    continue;
                }
                emailValue = email;
            }

            // ── Validate DepartmentId (optional) ──────────────────────────────
            Guid? departmentId = null;
            if (!string.IsNullOrWhiteSpace(deptIdStr))
            {
                if (!Guid.TryParse(deptIdStr, out var parsedDept))
                {
                    errors.Add($"Line {lineNumber}: DepartmentId '{deptIdStr}' is not a valid GUID.");
                    continue;
                }
                departmentId = parsedDept;
            }

            // ── Check intra-batch duplicate ────────────────────────────────────
            if (batchUsernames.Contains(username))
            {
                duplicates++;
                continue;
            }

            // ── Check existing DB duplicate ────────────────────────────────────
            if (await _userRepo.UsernameExistsAsync(username, ct))
            {
                duplicates++;
                continue;
            }

            // ── Resolve role ID (cached) ───────────────────────────────────────
            if (!roleCache.TryGetValue(roleName, out var roleId))
            {
                var role = await _userRepo.GetRoleByNameAsync(roleName, ct);
                if (role is null)
                {
                    errors.Add($"Line {lineNumber}: Role '{roleName}' not found in the database.");
                    continue;
                }
                roleCache[roleName] = role.Id;
                roleId = role.Id;
            }

            // ── Build user — initial password = username (P4-S2-01) ───────────
            var passwordHash = _hasher.Hash(username);
            var user = new User(
                username: username,
                passwordHash: passwordHash,
                roleId: roleId,
                email: emailValue,
                departmentId: departmentId,
                mustChangePassword: true   // P4-S2-02: force change on first login
            );

            batchUsernames.Add(username);
            toImport.Add(user);
        }

        if (toImport.Count > 0)
        {
            await _userRepo.AddRangeAsync(toImport, ct);
            await _userRepo.SaveChangesAsync(ct);
        }

        return new UserImportResult(
            TotalRows: totalRows,
            Imported: toImport.Count,
            Duplicates: duplicates,
            Errors: errors.Count,
            ErrorDetails: errors
        );
    }
}
