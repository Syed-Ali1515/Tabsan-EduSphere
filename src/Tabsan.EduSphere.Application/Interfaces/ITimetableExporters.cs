using Tabsan.EduSphere.Application.Dtos;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>Generates an Excel (.xlsx) timetable export. Implemented in Infrastructure using ClosedXML.</summary>
public interface ITimetableExcelExporter
{
    /// <summary>Returns the .xlsx file contents as a byte array.</summary>
    byte[] Export(TimetableDto timetable);
}

/// <summary>Generates a PDF timetable export. Implemented in Infrastructure using QuestPDF.</summary>
public interface ITimetablePdfExporter
{
    /// <summary>Returns the .pdf file contents as a byte array.</summary>
    byte[] Export(TimetableDto timetable);
}
