using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

// Phase 25 — Academic Engine Unification — Stage 25.2

/// <summary>EF Core configuration for the InstitutionGradingProfile entity.</summary>
public class InstitutionGradingProfileConfiguration : IEntityTypeConfiguration<InstitutionGradingProfile>
{
    public void Configure(EntityTypeBuilder<InstitutionGradingProfile> builder)
    {
        builder.ToTable("institution_grading_profiles");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.InstitutionType).IsRequired();
        builder.Property(p => p.PassThreshold).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(p => p.GradeRangesJson).HasColumnType("nvarchar(max)");
        builder.Property(p => p.IsActive).HasDefaultValue(true);

        // One active profile per institution type.
        builder.HasIndex(p => p.InstitutionType)
               .IsUnique()
               .HasDatabaseName("IX_institution_grading_profiles_type");
    }
}
