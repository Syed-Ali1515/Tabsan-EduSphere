// Final-Touches Phase 18 Stage 18.2 — Certificate generation abstraction

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Generates a PDF graduation certificate for a given student.
/// Implemented in Infrastructure using QuestPDF.
/// </summary>
public interface ICertificateGenerator
{
    /// <summary>
    /// Builds and returns the raw PDF bytes for a graduation certificate.
    /// </summary>
    /// <param name="studentName">Display name (Username) of the graduating student.</param>
    /// <param name="registrationNumber">Student's registration number.</param>
    /// <param name="programName">Academic programme the student is graduating from.</param>
    /// <param name="headline">Optional body text from portal settings; a default is used when null.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<byte[]> GeneratePdfAsync(
        string studentName,
        string registrationNumber,
        string programName,
        string? headline,
        CancellationToken ct = default);
}
