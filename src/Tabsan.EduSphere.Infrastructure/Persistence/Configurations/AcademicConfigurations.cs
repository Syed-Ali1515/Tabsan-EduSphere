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

        // Final-Touches Phase 17 Stage 17.3 — core/elective classification column
        builder.Property(c => c.CourseType)
               .IsRequired()
               .HasConversion<int>()
               .HasDefaultValue(Domain.Academic.CourseType.Core);

        // Final-Touches Phase 19 Stage 19.1 — semester-based flag
        builder.Property(c => c.HasSemesters)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(c => c.TotalSemesters);

        // Final-Touches Phase 19 Stage 19.2 — non-semester duration + grading type
        builder.Property(c => c.DurationUnit).HasMaxLength(20);

        builder.Property(c => c.GradingType)
               .IsRequired()
               .HasMaxLength(20)
               .HasDefaultValue("GPA");

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

// Final-Touches Phase 15 Stage 15.1 — CoursePrerequisiteConfiguration: prerequisite link table
/// <summary>EF Core configuration for the CoursePrerequisite join entity.</summary>
public class CoursePrerequisiteConfiguration : IEntityTypeConfiguration<CoursePrerequisite>
{
    public void Configure(EntityTypeBuilder<CoursePrerequisite> builder)
    {
        builder.ToTable("course_prerequisites");
        builder.HasKey(p => p.Id);

        // Unique constraint: a course can list each prerequisite at most once.
        builder.HasIndex(p => new { p.CourseId, p.PrerequisiteCourseId })
               .IsUnique()
               .HasDatabaseName("IX_course_prerequisites_course_prereq");

        builder.HasOne(p => p.Course)
               .WithMany()
               .HasForeignKey(p => p.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.PrerequisiteCourse)
               .WithMany()
               .HasForeignKey(p => p.PrerequisiteCourseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

// Final-Touches Phase 17 Stage 17.2 — degree rule EF configuration
/// <summary>EF Core configuration for DegreeRule.</summary>
public class DegreeRuleConfiguration : IEntityTypeConfiguration<DegreeRule>
{
    public void Configure(EntityTypeBuilder<DegreeRule> builder)
    {
        builder.ToTable("degree_rules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.MinGpa).HasPrecision(4, 2);
        builder.Property(r => r.RowVersion).IsRowVersion();

        // Only one rule per academic program.
        builder.HasIndex(r => r.AcademicProgramId)
               .IsUnique()
               .HasDatabaseName("IX_degree_rules_program");

        builder.HasOne(r => r.AcademicProgram)
               .WithMany()
               .HasForeignKey(r => r.AcademicProgramId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.RequiredCourses)
               .WithOne()
               .HasForeignKey(rc => rc.DegreeRuleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

// Final-Touches Phase 17 Stage 17.2 — required course join table configuration
/// <summary>EF Core configuration for DegreeRuleRequiredCourse.</summary>
public class DegreeRuleRequiredCourseConfiguration : IEntityTypeConfiguration<DegreeRuleRequiredCourse>
{
    public void Configure(EntityTypeBuilder<DegreeRuleRequiredCourse> builder)
    {
        builder.ToTable("degree_rule_required_courses");
        builder.HasKey(rc => rc.Id);

        // A course can be required at most once per rule.
        builder.HasIndex(rc => new { rc.DegreeRuleId, rc.CourseId })
               .IsUnique()
               .HasDatabaseName("IX_degree_rule_required_courses_rule_course");

        builder.HasOne(rc => rc.Course)
               .WithMany()
               .HasForeignKey(rc => rc.CourseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

// Final-Touches Phase 18 Stage 18.1 — GraduationApplication EF configuration
/// <summary>EF Core configuration for GraduationApplication.</summary>
public class GraduationApplicationConfiguration : IEntityTypeConfiguration<GraduationApplication>
{
    public void Configure(EntityTypeBuilder<GraduationApplication> builder)
    {
        builder.ToTable("graduation_applications");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).IsRequired().HasConversion<int>();
        builder.Property(a => a.StudentNote).HasMaxLength(2000);
        builder.Property(a => a.CertificatePath).HasMaxLength(500);
        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.HasOne(a => a.StudentProfile)
               .WithMany()
               .HasForeignKey(a => a.StudentProfileId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Approvals)
               .WithOne()
               .HasForeignKey(ap => ap.GraduationApplicationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

// Final-Touches Phase 18 Stage 18.1 — GraduationApplicationApproval EF configuration
/// <summary>EF Core configuration for GraduationApplicationApproval.</summary>
public class GraduationApplicationApprovalConfiguration : IEntityTypeConfiguration<GraduationApplicationApproval>
{
    public void Configure(EntityTypeBuilder<GraduationApplicationApproval> builder)
    {
        builder.ToTable("graduation_application_approvals");
        builder.HasKey(ap => ap.Id);
        builder.Property(ap => ap.Stage).IsRequired().HasConversion<int>();
        builder.Property(ap => ap.Note).HasMaxLength(1000);
    }
}

// Final-Touches Phase 19 Stage 19.4 — CourseGradingConfig EF configuration
/// <summary>EF Core configuration for CourseGradingConfig.</summary>
public class CourseGradingConfigConfiguration : IEntityTypeConfiguration<CourseGradingConfig>
{
    public void Configure(EntityTypeBuilder<CourseGradingConfig> builder)
    {
        builder.ToTable("course_grading_configs");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PassThreshold)
               .IsRequired()
               .HasColumnType("decimal(5,2)");

        builder.Property(c => c.GradingType)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(c => c.GradeRangesJson)
               .HasMaxLength(4000);

        // One course → one grading config
        builder.HasOne(c => c.Course)
               .WithMany()
               .HasForeignKey(c => c.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.CourseId)
               .IsUnique()
               .HasDatabaseName("IX_course_grading_configs_courseId");
    }
}
