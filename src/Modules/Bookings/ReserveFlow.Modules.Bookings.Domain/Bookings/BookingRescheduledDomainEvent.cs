using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Bookings;

public sealed record BookingRescheduledDomainEvent(
    Guid BookingId,
    Guid TenantId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : DomainEvent;
