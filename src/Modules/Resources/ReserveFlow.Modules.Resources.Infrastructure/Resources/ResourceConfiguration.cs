using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Infrastructure.Resources;

internal sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resources");

        builder.HasKey(resource => resource.Id);

        builder.Property(resource => resource.Id).HasColumnName("id");
        builder.Property(resource => resource.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(resource => resource.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(resource => resource.ResourceType).HasColumnName("resource_type").HasMaxLength(100).IsRequired();
        builder.Property(resource => resource.Capacity).HasColumnName("capacity").IsRequired();
        builder.Property(resource => resource.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(resource => resource.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(resource => resource.DomainEvents);

        builder.HasIndex(resource => new { resource.TenantId, resource.IsActive })
            .HasDatabaseName("ix_resources_tenant_active");

        builder.HasIndex(resource => new { resource.TenantId, resource.ResourceType })
            .HasDatabaseName("ix_resources_tenant_type");
    }
}
