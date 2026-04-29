using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.StudentLifecycle;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the TeacherModificationRequest entity.
/// Indexes applied on TeacherUserId, Status, and ModificationType for query performance.
/// </summary>
public class TeacherModificationRequestConfiguration : IEntityTypeConfiguration<TeacherModificationRequest>
{
    public void Configure(EntityTypeBuilder<TeacherModificationRequest> builder)
    {
        builder.ToTable("teacher_modification_requests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.TeacherUserId).IsRequired();
        builder.Property(x => x.ReviewedByUserId).IsRequired(false);
        builder.Property(x => x.ModificationType).IsRequired().HasConversion<int>();
        builder.Property(x => x.RecordId).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.ProposedData).IsRequired().HasColumnType("NVARCHAR(MAX)");
        builder.Property(x => x.AdminNotes).IsRequired(false).HasMaxLength(2000);
        builder.Property(x => x.ReviewedAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Foreign keys
        builder.HasOne(x => x.Teacher)
            .WithMany()
            .HasForeignKey(x => x.TeacherUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_tmr_teacher_user");

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_tmr_reviewer_user");

        // Indexes
        builder.HasIndex(x => x.TeacherUserId).HasDatabaseName("ix_tmr_teacher_user_id");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_tmr_status");
        builder.HasIndex(x => x.ModificationType).HasDatabaseName("ix_tmr_modification_type");
        builder.HasIndex(x => new { x.TeacherUserId, x.Status }).HasDatabaseName("ix_tmr_teacher_status");
        builder.HasIndex(x => x.RecordId).HasDatabaseName("ix_tmr_record_id");
    }
}
