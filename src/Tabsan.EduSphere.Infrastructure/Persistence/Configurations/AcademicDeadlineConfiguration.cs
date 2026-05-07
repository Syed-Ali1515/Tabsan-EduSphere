using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table mapping for AcademicDeadline.</summary>
public class AcademicDeadlineConfiguration : IEntityTypeConfiguration<AcademicDeadline>
{
    public void Configure(EntityTypeBuilder<AcademicDeadline> builder)
    {
        builder.ToTable("academic_deadlines");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Title).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(1000);
        builder.Property(d => d.RowVersion).IsRowVersion();

        builder.HasOne(d => d.Semester)
               .WithMany()
               .HasForeignKey(d => d.SemesterId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.SemesterId)
               .HasDatabaseName("IX_academic_deadlines_semester");

        builder.HasIndex(d => new { d.DeadlineDate, d.IsActive })
               .HasDatabaseName("IX_academic_deadlines_date_active");

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
