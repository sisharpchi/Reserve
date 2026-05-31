namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed record RescheduleBookingRequest(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);
