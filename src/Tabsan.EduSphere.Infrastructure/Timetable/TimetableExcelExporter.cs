using ClosedXML.Excel;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.Exporters;

/// <summary>
/// Generates a timetable Excel file using ClosedXML.
/// The output is a structured weekly grid:
///   - Columns: Time Slot, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
///   - Rows: one per unique time slot found in the timetable entries
/// </summary>
public class TimetableExcelExporter : ITimetableExcelExporter
{
    private static readonly string[] DayHeaders =
        ["Time Slot", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    public byte[] Export(TimetableDto timetable)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Timetable");

        // ── Title row ──────────────────────────────────────────────────────────
        sheet.Cell(1, 1).Value = $"{timetable.Title} — {timetable.DepartmentName} — {timetable.SemesterName}";
        var titleRange = sheet.Range(1, 1, 1, DayHeaders.Length);
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 14;
        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A5F");
        titleRange.Style.Font.FontColor = XLColor.White;

        // ── Header row ─────────────────────────────────────────────────────────
        int headerRow = 2;
        for (int col = 0; col < DayHeaders.Length; col++)
        {
            var cell = sheet.Cell(headerRow, col + 1);
            cell.Value = DayHeaders[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E5FA3");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // ── Build time-slot grid ───────────────────────────────────────────────
        // Collect distinct time slots sorted ascending
        var timeSlots = timetable.Entries
            .Select(e => (e.StartTime, e.EndTime))
            .Distinct()
            .OrderBy(s => s.StartTime)
            .ToList();

        int dataStartRow = headerRow + 1;

        for (int i = 0; i < timeSlots.Count; i++)
        {
            int row = dataStartRow + i;
            var (start, end) = timeSlots[i];

            // Time slot label
            var slotCell = sheet.Cell(row, 1);
            slotCell.Value = $"{start:HH\\:mm} – {end:HH\\:mm}";
            slotCell.Style.Font.Bold = true;
            slotCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F0F4FF");

            // Alternate row shading
            if (i % 2 != 0)
                sheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F9F9F9");

            // Fill in each day column
            for (int day = 0; day <= 6; day++)
            {
                var entry = timetable.Entries.FirstOrDefault(e =>
                    e.DayOfWeek == day &&
                    e.StartTime == start &&
                    e.EndTime == end);

                if (entry is not null)
                {
                    var parts = new List<string> { entry.SubjectName };
                    if (!string.IsNullOrWhiteSpace(entry.FacultyName)) parts.Add(entry.FacultyName);
                    if (!string.IsNullOrWhiteSpace(entry.RoomNumber)) parts.Add($"[{entry.RoomNumber}]");

                    var cell = sheet.Cell(row, day + 2); // column 2 = Sunday (day 0)
                    cell.Value = string.Join("\n", parts);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
            }
        }

        // ── Auto-fit columns and set minimum row heights ───────────────────────
        sheet.Columns().AdjustToContents();
        sheet.Column(1).Width = 16;
        for (int col = 2; col <= DayHeaders.Length; col++)
            sheet.Column(col).Width = Math.Max(sheet.Column(col).Width, 20);

        for (int row = dataStartRow; row < dataStartRow + timeSlots.Count; row++)
            sheet.Row(row).Height = 40;

        // ── Border around data area ────────────────────────────────────────────
        if (timeSlots.Count > 0)
        {
            var dataRange = sheet.Range(headerRow, 1, dataStartRow + timeSlots.Count - 1, DayHeaders.Length);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
