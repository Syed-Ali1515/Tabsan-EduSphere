using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the AcademicProgram entity.</summary>
public class AcademicProgramConfiguration : IEntityTypeConfiguration<AcademicProgram>
{
    public void Configure(EntityTypeBuilder<AcademicProgram> builder)
    {
        builder.ToTable("academic_programs");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(20);
        builder.Property(p => p.RowVersion).IsRowVersion();

        // Unique programme code per department.
        builder.HasIndex(p => p.Code)
               .IsUnique()
               .HasDatabaseName("IX_academic_programs_code");

        builder.HasOne(p => p.Department)
               .WithMany()
               .HasForeignKey(p => p.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

/// <summary>EF Core configuration for the Semester entity.</summary>
public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
        builder.ToTable("semesters");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}

/// <summary>EF Core configuration for Course and CourseOffering entities.</summary>
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.RowVersion).IsRowVersion();

        // Code is unique within a department.
        builder.HasIndex(c => new { c.Code, c.DepartmentId })
               .IsUnique()
               .HasDatabaseName("IX_courses_code_dept");

        builder.HasOne(c => c.Department)
               .WithMany()
               .HasForeignKey(c => c.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

/// <summary>EF Core configuration for CourseOffering.</summary>
public class CourseOfferingConfiguration : IEntityTypeConfiguration<CourseOffering>
{
    public void Configure(EntityTypeBuilder<CourseOffering> builder)
    {
        builder.ToTable("course_offerings");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.RowVersion).IsRowVersion();

        // A course cannot be offered more than once per semester.
        builder.HasIndex(o => new { o.CourseId, o.SemesterId })
               .IsUnique()
               .HasDatabaseName("IX_course_offerings_course_semester");

        builder.HasOne(o => o.Course)
               .WithMany()
               .HasForeignKey(o => o.CourseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Semester)
               .WithMany()
               .HasForeignKey(o => o.SemesterId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
