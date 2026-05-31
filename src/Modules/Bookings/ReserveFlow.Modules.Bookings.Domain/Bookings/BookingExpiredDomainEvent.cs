using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Bookings;

public sealed record BookingExpiredDomainEvent(
    Guid BookingId,
    Guid TenantId,
    DateTimeOffset ExpiredAtUtc) : DomainEvent;
