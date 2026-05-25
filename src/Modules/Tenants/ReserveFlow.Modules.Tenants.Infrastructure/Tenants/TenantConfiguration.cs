using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Infrastructure.Tenants;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(tenant => tenant.Id);

        builder.Property(tenant => tenant.Id).HasColumnName("id");
        builder.Property(tenant => tenant.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(tenant => tenant.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.Property(tenant => tenant.TimeZoneId).HasColumnName("time_zone_id").HasMaxLength(100).IsRequired();
        builder.Property(tenant => tenant.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(tenant => tenant.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(tenant => tenant.DomainEvents);

        builder.HasIndex(tenant => tenant.Slug)
            .IsUnique()
            .HasDatabaseName("ux_tenants_slug");

        builder.HasIndex(tenant => tenant.Status)
            .HasDatabaseName("ix_tenants_status");
    }
}
