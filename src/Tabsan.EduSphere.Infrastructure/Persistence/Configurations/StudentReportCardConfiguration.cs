using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

// Phase 26 — Stage 26.2

public class StudentReportCardConfiguration : IEntityTypeConfiguration<StudentReportCard>
{
    public void Configure(EntityTypeBuilder<StudentReportCard> builder)
    {
        builder.ToTable("student_report_cards");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentProfileId).IsRequired();
        builder.Property(x => x.InstitutionType).IsRequired();

        builder.Property(x => x.PeriodLabel)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.PayloadJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.GeneratedByUserId).IsRequired();
        builder.Property(x => x.GeneratedAt).IsRequired();

        builder.HasIndex(x => new { x.StudentProfileId, x.GeneratedAt })
            .HasDatabaseName("IX_student_report_cards_student_generated");
    }
}
