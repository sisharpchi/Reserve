using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Infrastructure.Services;

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(service => service.Id);

        builder.Property(service => service.Id).HasColumnName("id");
        builder.Property(service => service.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(service => service.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(service => service.DurationMinutes).HasColumnName("duration_minutes").IsRequired();
        builder.Property(service => service.Price).HasColumnName("price").HasPrecision(18, 2);
        builder.Property(service => service.Currency).HasColumnName("currency").HasMaxLength(10);
        builder.Property(service => service.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(service => service.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(service => service.DomainEvents);

        builder.HasIndex(service => new { service.TenantId, service.IsActive })
            .HasDatabaseName("ix_services_tenant_active");
    }
}
