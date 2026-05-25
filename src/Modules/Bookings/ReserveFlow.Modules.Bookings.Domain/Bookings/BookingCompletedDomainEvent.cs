using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Bookings;

public sealed record BookingCompletedDomainEvent(
    Guid BookingId,
    Guid TenantId,
    DateTimeOffset CompletedAtUtc) : DomainEvent;
