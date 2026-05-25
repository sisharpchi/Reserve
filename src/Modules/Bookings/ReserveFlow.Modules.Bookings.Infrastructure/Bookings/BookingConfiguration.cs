using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Bookings;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.Id).HasColumnName("id");
        builder.Property(booking => booking.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(booking => booking.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(booking => booking.ServiceId).HasColumnName("service_id").IsRequired();
        builder.Property(booking => booking.StaffMemberId).HasColumnName("staff_member_id");
        builder.Property(booking => booking.ResourceId).HasColumnName("resource_id");
        builder.Property(booking => booking.StartsAtUtc).HasColumnName("starts_at_utc").IsRequired();
        builder.Property(booking => booking.EndsAtUtc).HasColumnName("ends_at_utc").IsRequired();
        builder.Property(booking => booking.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(booking => booking.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(booking => booking.ConcurrencyToken).HasColumnName("concurrency_token").IsConcurrencyToken().IsRequired();

        builder.Ignore(booking => booking.DomainEvents);

        builder.HasIndex(booking => new { booking.TenantId, booking.StaffMemberId, booking.StartsAtUtc, booking.EndsAtUtc })
            .HasDatabaseName("ix_bookings_tenant_staff_time");

        builder.HasIndex(booking => new { booking.TenantId, booking.ResourceId, booking.StartsAtUtc, booking.EndsAtUtc })
            .HasDatabaseName("ix_bookings_tenant_resource_time");

        builder.HasIndex(booking => new { booking.TenantId, booking.Status, booking.StartsAtUtc })
            .HasDatabaseName("ix_bookings_tenant_status_time");

        builder.HasIndex(booking => new { booking.TenantId, booking.IdempotencyKey })
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL")
            .HasDatabaseName("ux_bookings_tenant_idempotency_key");
    }
}
