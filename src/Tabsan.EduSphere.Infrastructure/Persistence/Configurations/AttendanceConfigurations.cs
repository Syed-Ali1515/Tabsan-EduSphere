using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Attendance;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the AttendanceRecord entity.</summary>
public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_records");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Remarks).HasMaxLength(500);

        // Enforce one record per (student, offering, date) — core business invariant.
        builder.HasIndex(a => new { a.StudentProfileId, a.CourseOfferingId, a.Date })
               .IsUnique()
               .HasDatabaseName("IX_attendance_student_offering_date");

        // Fast per-offering queries (faculty takes attendance for a whole class).
        builder.HasIndex(a => new { a.CourseOfferingId, a.Date })
               .HasDatabaseName("IX_attendance_offering_date");

        // Fast per-student queries (student views own attendance history).
        builder.HasIndex(a => a.StudentProfileId)
               .HasDatabaseName("IX_attendance_student_id");

        // Attendance records are permanent — no soft-delete.
    }
}
