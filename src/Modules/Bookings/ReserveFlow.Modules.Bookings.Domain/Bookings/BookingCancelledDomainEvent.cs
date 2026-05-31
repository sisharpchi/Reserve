using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Bookings;

public sealed record BookingCancelledDomainEvent(
    Guid BookingId,
    Guid TenantId,
    DateTimeOffset CancelledAtUtc) : DomainEvent;
