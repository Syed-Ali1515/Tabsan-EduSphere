// Final-Touches Phase 22 Stage 22.2 — AccreditationTemplate EF Core configuration
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Settings;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table mapping for <see cref="AccreditationTemplate"/>.</summary>
public class AccreditationTemplateConfiguration : IEntityTypeConfiguration<AccreditationTemplate>
{
    public void Configure(EntityTypeBuilder<AccreditationTemplate> builder)
    {
        builder.ToTable("accreditation_templates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name)
               .IsRequired()
               .HasMaxLength(200);
        builder.Property(t => t.Description)
               .HasMaxLength(500)
               .IsRequired(false);
        builder.Property(t => t.Format)
               .IsRequired()
               .HasMaxLength(10);
        builder.Property(t => t.FieldMappingsJson)
               .HasMaxLength(2000)
               .IsRequired(false);
        builder.Property(t => t.IsActive)
               .IsRequired()
               .HasDefaultValue(true);
        builder.HasIndex(t => t.Name)
               .HasDatabaseName("IX_accreditation_templates_name");
    }
}
