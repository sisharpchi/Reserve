using ReserveFlow.Common.Domain;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Domain.BookingHistory;

public sealed class BookingHistoryEntry : Entity
{
    private BookingHistoryEntry(
        Guid id,
        Guid tenantId,
        Guid bookingId,
        BookingStatus status,
        DateTimeOffset changedAtUtc,
        string? reason)
        : base(id)
    {
        TenantId = tenantId;
        BookingId = bookingId;
        Status = status;
        ChangedAtUtc = changedAtUtc;
        Reason = reason;
    }

    private BookingHistoryEntry()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid BookingId { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTimeOffset ChangedAtUtc { get; private set; }

    public string? Reason { get; private set; }

    public static BookingHistoryEntry Record(
        Guid tenantId,
        Guid bookingId,
        BookingStatus status,
        DateTimeOffset changedAtUtc,
        string? reason)
    {
        var historyEntry = new BookingHistoryEntry(
            Guid.NewGuid(),
            tenantId,
            bookingId,
            status,
            changedAtUtc,
            NormalizeReason(reason));

        historyEntry.RaiseDomainEvent(new BookingHistoryRecordedDomainEvent(
            historyEntry.Id,
            tenantId,
            bookingId,
            status,
            changedAtUtc));

        return historyEntry;
    }

    private static string? NormalizeReason(string? reason)
    {
        return string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }
}
