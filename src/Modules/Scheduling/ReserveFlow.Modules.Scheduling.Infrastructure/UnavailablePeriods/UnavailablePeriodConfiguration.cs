using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;

namespace ReserveFlow.Modules.Scheduling.Infrastructure.UnavailablePeriods;

internal sealed class UnavailablePeriodConfiguration : IEntityTypeConfiguration<UnavailablePeriod>
{
    public void Configure(EntityTypeBuilder<UnavailablePeriod> builder)
    {
        builder.ToTable("unavailable_periods");

        builder.HasKey(unavailablePeriod => unavailablePeriod.Id);

        builder.Property(unavailablePeriod => unavailablePeriod.Id).HasColumnName("id");
        builder.Property(unavailablePeriod => unavailablePeriod.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(unavailablePeriod => unavailablePeriod.StaffMemberId).HasColumnName("staff_member_id");
        builder.Property(unavailablePeriod => unavailablePeriod.ResourceId).HasColumnName("resource_id");
        builder.Property(unavailablePeriod => unavailablePeriod.StartsAtUtc).HasColumnName("starts_at_utc").IsRequired();
        builder.Property(unavailablePeriod => unavailablePeriod.EndsAtUtc).HasColumnName("ends_at_utc").IsRequired();
        builder.Property(unavailablePeriod => unavailablePeriod.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(unavailablePeriod => unavailablePeriod.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(unavailablePeriod => unavailablePeriod.DomainEvents);

        builder.HasIndex(unavailablePeriod => new
            {
                unavailablePeriod.TenantId,
                unavailablePeriod.StaffMemberId,
                unavailablePeriod.StartsAtUtc,
                unavailablePeriod.EndsAtUtc
            })
            .HasDatabaseName("ix_unavailable_periods_tenant_staff_time");

        builder.HasIndex(unavailablePeriod => new
            {
                unavailablePeriod.TenantId,
                unavailablePeriod.ResourceId,
                unavailablePeriod.StartsAtUtc,
                unavailablePeriod.EndsAtUtc
            })
            .HasDatabaseName("ix_unavailable_periods_tenant_resource_time");
    }
}
