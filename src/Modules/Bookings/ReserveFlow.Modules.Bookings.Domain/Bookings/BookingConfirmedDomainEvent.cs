using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Bookings;

public sealed record BookingConfirmedDomainEvent(
    Guid BookingId,
    Guid TenantId,
    DateTimeOffset ConfirmedAtUtc) : DomainEvent;
