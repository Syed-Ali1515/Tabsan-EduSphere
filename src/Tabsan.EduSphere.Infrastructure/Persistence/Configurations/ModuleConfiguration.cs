using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Modules;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configurations for Module and ModuleStatus tables.</summary>
public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("modules");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Key).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);

        // Module key is used in policy checks throughout the system — must be unique.
        builder.HasIndex(m => m.Key)
               .IsUnique()
               .HasDatabaseName("IX_modules_key");
    }
}

/// <summary>EF Core configuration for the ModuleStatus join table.</summary>
public class ModuleStatusConfiguration : IEntityTypeConfiguration<ModuleStatus>
{
    public void Configure(EntityTypeBuilder<ModuleStatus> builder)
    {
        builder.ToTable("module_status");
        builder.HasKey(ms => ms.Id);

        builder.Property(ms => ms.Source).IsRequired().HasMaxLength(20);

        // One status row per module — unique index prevents duplicate status rows.
        builder.HasIndex(ms => ms.ModuleId)
               .IsUnique()
               .HasDatabaseName("IX_module_status_module_id");

        builder.HasOne(ms => ms.Module)
               .WithMany()
               .HasForeignKey(ms => ms.ModuleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
