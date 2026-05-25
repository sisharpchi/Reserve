using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Bookings;

public sealed record BookingMarkedAsNoShowDomainEvent(
    Guid BookingId,
    Guid TenantId,
    DateTimeOffset MarkedAtUtc) : DomainEvent;
