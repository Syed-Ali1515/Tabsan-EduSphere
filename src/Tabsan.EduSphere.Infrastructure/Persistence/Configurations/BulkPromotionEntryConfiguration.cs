using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

// Phase 26 — Stage 26.2

public class BulkPromotionEntryConfiguration : IEntityTypeConfiguration<BulkPromotionEntry>
{
    public void Configure(EntityTypeBuilder<BulkPromotionEntry> builder)
    {
        builder.ToTable("bulk_promotion_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BatchId).IsRequired();
        builder.Property(x => x.StudentProfileId).IsRequired();
        builder.Property(x => x.Decision).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500);

        builder.HasIndex(x => x.BatchId)
            .HasDatabaseName("IX_bulk_promotion_entries_batch");

        builder.HasIndex(x => new { x.BatchId, x.StudentProfileId })
            .IsUnique()
            .HasDatabaseName("IX_bulk_promotion_entries_batch_student");
    }
}
