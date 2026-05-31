using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;

namespace ReserveFlow.Modules.Bookings.Infrastructure.BookingPolicies;

internal sealed class BookingPolicyConfiguration : IEntityTypeConfiguration<BookingPolicy>
{
    public void Configure(EntityTypeBuilder<BookingPolicy> builder)
    {
        builder.ToTable("booking_policies");

        builder.HasKey(policy => policy.Id);

        builder.Property(policy => policy.Id).HasColumnName("id");
        builder.Property(policy => policy.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(policy => policy.MinimumAdvanceMinutes).HasColumnName("minimum_advance_minutes").IsRequired();
        builder.Property(policy => policy.CancellationDeadlineHours).HasColumnName("cancellation_deadline_hours").IsRequired();

        builder.Ignore(policy => policy.DomainEvents);

        builder.HasIndex(policy => policy.TenantId)
            .IsUnique()
            .HasDatabaseName("ux_booking_policies_tenant");
    }
}
