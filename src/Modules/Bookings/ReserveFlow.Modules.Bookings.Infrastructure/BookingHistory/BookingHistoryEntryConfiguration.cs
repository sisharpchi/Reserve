using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;

namespace ReserveFlow.Modules.Bookings.Infrastructure.BookingHistory;

internal sealed class BookingHistoryEntryConfiguration : IEntityTypeConfiguration<BookingHistoryEntry>
{
    public void Configure(EntityTypeBuilder<BookingHistoryEntry> builder)
    {
        builder.ToTable("booking_history");

        builder.HasKey(historyEntry => historyEntry.Id);

        builder.Property(historyEntry => historyEntry.Id).HasColumnName("id");
        builder.Property(historyEntry => historyEntry.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(historyEntry => historyEntry.BookingId).HasColumnName("booking_id").IsRequired();
        builder.Property(historyEntry => historyEntry.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(historyEntry => historyEntry.ChangedAtUtc).HasColumnName("changed_at_utc").IsRequired();
        builder.Property(historyEntry => historyEntry.Reason).HasColumnName("reason").HasMaxLength(500);

        builder.Ignore(historyEntry => historyEntry.DomainEvents);

        builder.HasIndex(historyEntry => new { historyEntry.TenantId, historyEntry.BookingId, historyEntry.ChangedAtUtc })
            .HasDatabaseName("ix_booking_history_tenant_booking_time");
    }
}
