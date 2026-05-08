// Final-Touches Phase 18 Stage 18.2 — QuestPDF certificate generator (Infrastructure)

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.Services;

/// <summary>
/// Generates graduation certificate PDFs using QuestPDF.
/// Registered in DI as a scoped service implementing ICertificateGenerator.
/// </summary>
public class CertificateGenerator : ICertificateGenerator
{
    private const string DefaultHeadline =
        "This is to certify that the above-named student has successfully completed all requirements for graduation.";

    public Task<byte[]> GeneratePdfAsync(
        string studentName,
        string registrationNumber,
        string programName,
        string? headline,
        CancellationToken ct = default)
    {
        // Final-Touches Phase 18 Stage 18.2 — Community license set once per call (idempotent)
        QuestPDF.Settings.License = LicenseType.Community;

        var body = string.IsNullOrWhiteSpace(headline) ? DefaultHeadline : headline;

        byte[] pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(13));

                page.Content().Column(col =>
                {
                    col.Spacing(15);

                    col.Item().AlignCenter().Text("GRADUATION CERTIFICATE")
                        .Bold().FontSize(28).FontColor(Colors.Blue.Darken3);

                    col.Item().AlignCenter().Text("TABSAN EDUSPHERE UNIVERSITY")
                        .FontSize(16).FontColor(Colors.Grey.Darken2);

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                    col.Item().AlignCenter().PaddingTop(10)
                        .Text("This is to certify that").FontSize(14).Italic();

                    col.Item().AlignCenter().Text(studentName)
                        .Bold().FontSize(22).FontColor(Colors.Black);

                    col.Item().AlignCenter().Text($"Registration No: {registrationNumber}")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);

                    col.Item().AlignCenter().Text("has successfully completed the")
                        .FontSize(14).Italic();

                    col.Item().AlignCenter().Text(programName)
                        .Bold().FontSize(18);

                    col.Item().AlignCenter().PaddingTop(5).Text(body)
                        .FontSize(12).FontColor(Colors.Grey.Darken1);

                    col.Item().AlignCenter().PaddingTop(20)
                        .Text($"Date: {DateTime.UtcNow:MMMM dd, yyyy}")
                        .FontSize(12);

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });
            });
        }).GeneratePdf();

        return Task.FromResult(pdf);
    }
}
