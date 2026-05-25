using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Bookings;

public sealed record BookingCreatedDomainEvent(
    Guid BookingId,
    Guid TenantId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : DomainEvent;
