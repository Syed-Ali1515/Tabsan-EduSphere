using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// A single scheduled slot within a timetable (e.g. Monday 08:00-09:30, CS101, Room 301, CS-Block).
/// References Course, User (faculty), Room, and Building via FK so dropdown selection is preserved.
/// String fields SubjectName, FacultyName, and RoomNumber are stored as display snapshots.
/// </summary>
public class TimetableEntry : BaseEntity
{
    /// <summary>FK to the parent timetable.</summary>
    public Guid TimetableId { get; private set; }

    /// <summary>Navigation to the parent timetable.</summary>
    public Timetable Timetable { get; private set; } = default!;

    /// <summary>Day of the week (0=Sunday ... 6=Saturday).</summary>
    public int DayOfWeek { get; private set; }

    /// <summary>Slot start time (time-of-day, no date component).</summary>
    public TimeOnly StartTime { get; private set; }

    /// <summary>Slot end time (time-of-day, no date component).</summary>
    public TimeOnly EndTime { get; private set; }

    /// <summary>FK to the Course selected from the dropdown. Null when entered as free text.</summary>
    public Guid? CourseId { get; private set; }

    /// <summary>Navigation to the course (subject).</summary>
    public Course? Course { get; private set; }

    /// <summary>Display snapshot of subject name. Populated from Course.Title when CourseId is set.</summary>
    public string SubjectName { get; private set; } = default!;

    /// <summary>FK to the faculty User assigned to teach this slot.</summary>
    public Guid? FacultyUserId { get; private set; }

    /// <summary>Display snapshot of the faculty member's name.</summary>
    public string? FacultyName { get; private set; }

    /// <summary>FK to the Room selected from the dropdown catalogue.</summary>
    public Guid? RoomId { get; private set; }

    /// <summary>Navigation to the room.</summary>
    public Room? Room { get; private set; }

    /// <summary>Display snapshot of the room number (e.g. "101", "CS-Lab-2").</summary>
    public string? RoomNumber { get; private set; }

    /// <summary>FK to the Building. Denormalized from Room.BuildingId for fast teacher-filtered queries.</summary>
    public Guid? BuildingId { get; private set; }

    /// <summary>Navigation to the building.</summary>
    public Building? Building { get; private set; }

    private TimetableEntry() { }

    public TimetableEntry(
        Guid timetableId,
        int dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        string subjectName,
        Guid? courseId = null,
        Guid? facultyUserId = null,
        string? facultyName = null,
        Guid? roomId = null,
        string? roomNumber = null,
        Guid? buildingId = null)
    {
        TimetableId = timetableId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        SubjectName = subjectName.Trim();
        CourseId = courseId;
        FacultyUserId = facultyUserId;
        FacultyName = facultyName?.Trim();
        RoomId = roomId;
        RoomNumber = roomNumber?.Trim();
        BuildingId = buildingId;
    }

    /// <summary>Replaces all mutable fields of this entry.</summary>
    public void Update(
        int dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        string subjectName,
        Guid? courseId,
        Guid? facultyUserId,
        string? facultyName,
        Guid? roomId,
        string? roomNumber,
        Guid? buildingId)
    {
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        SubjectName = subjectName.Trim();
        CourseId = courseId;
        FacultyUserId = facultyUserId;
        FacultyName = facultyName?.Trim();
        RoomId = roomId;
        RoomNumber = roomNumber?.Trim();
        BuildingId = buildingId;
    }
}
