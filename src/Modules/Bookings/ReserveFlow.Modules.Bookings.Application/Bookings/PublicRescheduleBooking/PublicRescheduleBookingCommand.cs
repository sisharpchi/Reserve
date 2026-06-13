using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.PublicRescheduleBooking;

public sealed record PublicRescheduleBookingCommand(
    Guid TenantId,
    string PublicReference,
    string AccessToken,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    DateTimeOffset RescheduledAtUtc) : ICommand<BookingResponse?>;
