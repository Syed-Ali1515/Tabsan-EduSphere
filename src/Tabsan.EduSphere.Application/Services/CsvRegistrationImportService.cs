using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Parses CSV uploads and bulk-inserts registration numbers into the whitelist.
/// CSV format (with header): RegistrationNumber,DepartmentId,ProgramId
/// Rules:
/// - Skip rows with missing or malformed data (report as errors)
/// - Skip rows whose RegistrationNumber already exists in whitelist (report as duplicates)
/// - Import valid rows atomically (all or none per batch)
/// </summary>
public class CsvRegistrationImportService : ICsvRegistrationImportService
{
    private readonly IRegistrationWhitelistRepository _whitelist;

    public CsvRegistrationImportService(IRegistrationWhitelistRepository whitelist)
    {
        _whitelist = whitelist;
    }

    public async Task<CsvImportResult> ImportFromCsvAsync(
        Stream csvStream,
        CancellationToken ct = default)
    {
        var errors = new List<string>();
        var toImport = new List<RegistrationWhitelist>();
        int totalRows = 0, duplicates = 0;

        using var reader = new StreamReader(csvStream);

        // Skip header line
        var header = await reader.ReadLineAsync(ct);
        if (header is null)
            return new CsvImportResult(0, 0, 0, 0, new List<string>());

        int lineNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            lineNumber++;
            totalRows++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',');
            if (parts.Length < 3)
            {
                errors.Add($"Line {lineNumber}: Expected 3 columns (RegistrationNumber,DepartmentId,ProgramId). Got {parts.Length}.");
                continue;
            }

            var regNo = parts[0].Trim();
            var deptIdStr = parts[1].Trim();
            var progIdStr = parts[2].Trim();

            if (string.IsNullOrWhiteSpace(regNo))
            {
                errors.Add($"Line {lineNumber}: RegistrationNumber is empty.");
                continue;
            }

            if (!Guid.TryParse(deptIdStr, out var deptId))
            {
                errors.Add($"Line {lineNumber}: Invalid DepartmentId '{deptIdStr}'.");
                continue;
            }

            if (!Guid.TryParse(progIdStr, out var progId))
            {
                errors.Add($"Line {lineNumber}: Invalid ProgramId '{progIdStr}'.");
                continue;
            }

            // Check for duplicate in current batch
            if (toImport.Any(e => e.IdentifierValue == regNo.ToLowerInvariant()))
            {
                duplicates++;
                continue;
            }

            // Check existing whitelist
            var existing = await _whitelist.FindUnusedAsync(regNo, ct);
            if (existing != null)
            {
                duplicates++;
                continue;
            }

            toImport.Add(new RegistrationWhitelist(
                WhitelistIdentifierType.RegistrationNumber,
                regNo,
                deptId,
                progId
            ));
        }

        if (toImport.Count > 0)
        {
            await _whitelist.AddRangeAsync(toImport, ct);
            await _whitelist.SaveChangesAsync(ct);
        }

        return new CsvImportResult(
            TotalRows: totalRows,
            Imported: toImport.Count,
            Duplicates: duplicates,
            Errors: errors.Count,
            ErrorDetails: errors
        );
    }

    public async Task<bool> AddSingleAsync(
        string registrationNumber,
        Guid departmentId,
        Guid programId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
            return false;

        var existing = await _whitelist.FindUnusedAsync(registrationNumber, ct);
        if (existing != null)
            return false;

        var entry = new RegistrationWhitelist(
            WhitelistIdentifierType.RegistrationNumber,
            registrationNumber,
            departmentId,
            programId
        );

        await _whitelist.AddAsync(entry, ct);
        await _whitelist.SaveChangesAsync(ct);
        return true;
    }
}
