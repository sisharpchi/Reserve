namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed record PublicRescheduleBookingRequest(
    string AccessToken,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);
