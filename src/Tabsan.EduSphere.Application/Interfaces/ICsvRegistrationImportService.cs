using Tabsan.EduSphere.Application.Dtos;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Service for importing student registration numbers in bulk via CSV.
/// Validates each row, detects duplicates, and inserts into the whitelist table.
/// </summary>
public interface ICsvRegistrationImportService
{
    /// <summary>
    /// Parses a CSV stream and imports valid registration numbers into the whitelist.
    /// CSV must have header row: RegistrationNumber,DepartmentId,ProgramId
    /// </summary>
    Task<CsvImportResult> ImportFromCsvAsync(
        Stream csvStream,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a single registration number to the whitelist.
    /// Returns false if the registration number already exists (whether used or not).
    /// </summary>
    Task<bool> AddSingleAsync(
        string registrationNumber,
        Guid departmentId,
        Guid programId,
        CancellationToken ct = default);
}
