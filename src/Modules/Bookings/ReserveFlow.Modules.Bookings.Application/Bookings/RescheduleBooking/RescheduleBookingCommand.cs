using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.RescheduleBooking;

public sealed record RescheduleBookingCommand(
    Guid BookingId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : ICommand<BookingResponse>;
