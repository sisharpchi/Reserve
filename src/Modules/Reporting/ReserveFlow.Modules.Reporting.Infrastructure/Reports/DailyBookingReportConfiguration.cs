using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Reporting.Domain.Reports;

namespace ReserveFlow.Modules.Reporting.Infrastructure.Reports;

internal sealed class DailyBookingReportConfiguration : IEntityTypeConfiguration<DailyBookingReport>
{
    public void Configure(EntityTypeBuilder<DailyBookingReport> builder)
    {
        builder.ToTable("daily_booking_reports");

        builder.HasKey(report => report.Id);

        builder.Property(report => report.Id).HasColumnName("id");
        builder.Property(report => report.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(report => report.Date).HasColumnName("date").HasColumnType("date").IsRequired();
        builder.Property(report => report.CreatedBookings).HasColumnName("created_bookings").IsRequired();
        builder.Property(report => report.CancelledBookings).HasColumnName("cancelled_bookings").IsRequired();
        builder.Property(report => report.CompletedBookings).HasColumnName("completed_bookings").IsRequired();
        builder.Property(report => report.NoShowBookings).HasColumnName("no_show_bookings").IsRequired();
        builder.Property(report => report.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();

        builder.Ignore(report => report.DomainEvents);

        builder.HasIndex(report => new { report.TenantId, report.Date })
            .IsUnique()
            .HasDatabaseName("ux_daily_booking_reports_tenant_date");
    }
}
