using ReserveFlow.Common.Domain;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Domain.BookingHistory;

public sealed record BookingHistoryRecordedDomainEvent(
    Guid BookingHistoryEntryId,
    Guid TenantId,
    Guid BookingId,
    BookingStatus Status,
    DateTimeOffset ChangedAtUtc) : DomainEvent;
