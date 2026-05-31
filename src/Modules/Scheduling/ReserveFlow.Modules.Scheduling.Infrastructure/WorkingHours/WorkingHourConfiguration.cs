using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;

namespace ReserveFlow.Modules.Scheduling.Infrastructure.WorkingHours;

internal sealed class WorkingHourConfiguration : IEntityTypeConfiguration<WorkingHour>
{
    public void Configure(EntityTypeBuilder<WorkingHour> builder)
    {
        builder.ToTable("working_hours");

        builder.HasKey(workingHour => workingHour.Id);

        builder.Property(workingHour => workingHour.Id).HasColumnName("id");
        builder.Property(workingHour => workingHour.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(workingHour => workingHour.StaffMemberId).HasColumnName("staff_member_id");
        builder.Property(workingHour => workingHour.ResourceId).HasColumnName("resource_id");
        builder.Property(workingHour => workingHour.DayOfWeek).HasColumnName("day_of_week").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(workingHour => workingHour.StartsAt).HasColumnName("starts_at").IsRequired();
        builder.Property(workingHour => workingHour.EndsAt).HasColumnName("ends_at").IsRequired();
        builder.Property(workingHour => workingHour.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(workingHour => workingHour.DomainEvents);

        builder.HasIndex(workingHour => new { workingHour.TenantId, workingHour.StaffMemberId, workingHour.DayOfWeek })
            .HasDatabaseName("ix_working_hours_tenant_staff_day");

        builder.HasIndex(workingHour => new { workingHour.TenantId, workingHour.ResourceId, workingHour.DayOfWeek })
            .HasDatabaseName("ix_working_hours_tenant_resource_day");
    }
}
