using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

// Phase 26 — Stage 26.1

public class SchoolStreamConfiguration : IEntityTypeConfiguration<SchoolStream>
{
    public void Configure(EntityTypeBuilder<SchoolStream> builder)
    {
        builder.ToTable("school_streams");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("IX_school_streams_name");
    }
}
