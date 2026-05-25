using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.CreateBooking;

public sealed record CreateBookingCommand(
    Guid TenantId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhoneNumber,
    string? IdempotencyKey,
    Guid ServiceId,
    Guid? StaffMemberId,
    Guid? ResourceId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : ICommand<BookingResponse>;
