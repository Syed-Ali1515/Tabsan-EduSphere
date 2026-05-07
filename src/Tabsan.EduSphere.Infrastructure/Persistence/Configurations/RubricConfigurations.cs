// Final-Touches Phase 16 Stage 16.2 — EF Core table configurations for rubric entities

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Assignments;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the Rubric entity.</summary>
public class RubricConfiguration : IEntityTypeConfiguration<Rubric>
{
    public void Configure(EntityTypeBuilder<Rubric> builder)
    {
        // Final-Touches Phase 16 Stage 16.2 — rubric table
        builder.ToTable("rubrics");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(300);

        // Only one active rubric per assignment
        builder.HasIndex(r => new { r.AssignmentId, r.IsActive })
               .HasDatabaseName("IX_rubrics_assignment_active");

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

/// <summary>EF Core configuration for the RubricCriterion entity.</summary>
public class RubricCriterionConfiguration : IEntityTypeConfiguration<RubricCriterion>
{
    public void Configure(EntityTypeBuilder<RubricCriterion> builder)
    {
        // Final-Touches Phase 16 Stage 16.2 — rubric_criteria table
        builder.ToTable("rubric_criteria");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(300);
        builder.Property(c => c.MaxPoints).HasColumnType("decimal(8,2)");

        builder.HasIndex(c => c.RubricId)
               .HasDatabaseName("IX_rubric_criteria_rubric_id");
    }
}

/// <summary>EF Core configuration for the RubricLevel entity.</summary>
public class RubricLevelConfiguration : IEntityTypeConfiguration<RubricLevel>
{
    public void Configure(EntityTypeBuilder<RubricLevel> builder)
    {
        // Final-Touches Phase 16 Stage 16.2 — rubric_levels table
        builder.ToTable("rubric_levels");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Label).IsRequired().HasMaxLength(200);
        builder.Property(l => l.PointsAwarded).HasColumnType("decimal(8,2)");

        builder.HasIndex(l => l.CriterionId)
               .HasDatabaseName("IX_rubric_levels_criterion_id");
    }
}

/// <summary>EF Core configuration for the RubricStudentGrade entity.</summary>
public class RubricStudentGradeConfiguration : IEntityTypeConfiguration<RubricStudentGrade>
{
    public void Configure(EntityTypeBuilder<RubricStudentGrade> builder)
    {
        // Final-Touches Phase 16 Stage 16.2 — rubric_student_grades table
        builder.ToTable("rubric_student_grades");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.PointsAwarded).HasColumnType("decimal(8,2)");

        // One grade per (submission, criterion) — enforce uniqueness
        builder.HasIndex(g => new { g.AssignmentSubmissionId, g.RubricCriterionId })
               .IsUnique()
               .HasDatabaseName("IX_rubric_student_grades_submission_criterion");

        builder.HasIndex(g => g.AssignmentSubmissionId)
               .HasDatabaseName("IX_rubric_student_grades_submission_id");
    }
}
