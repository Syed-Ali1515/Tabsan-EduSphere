namespace Tabsan.EduSphere.Application.Dtos;

/// <summary>
/// Result of a CSV import operation for the registration whitelist.
/// Contains successes, duplicates, and validation errors.
/// </summary>
public record CsvImportResult(
    int TotalRows,
    int Imported,
    int Duplicates,
    int Errors,
    IList<string> ErrorDetails
);

/// <summary>
/// A single row from a registration number CSV import.
/// Required columns: RegistrationNumber, DepartmentId, ProgramId.
/// </summary>
public record CsvWhitelistRow(
    string RegistrationNumber,
    string DepartmentId,
    string ProgramId
);
