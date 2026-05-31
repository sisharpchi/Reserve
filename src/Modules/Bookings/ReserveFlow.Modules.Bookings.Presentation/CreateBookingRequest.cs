namespace ReserveFlow.Modules.Bookings.Presentation;

public sealed record CreateBookingRequest(
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhoneNumber,
    string? IdempotencyKey,
    Guid ServiceId,
    Guid? StaffMemberId,
    Guid? ResourceId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);
