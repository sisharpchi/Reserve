using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.ConfirmBooking;

public sealed record ConfirmBookingCommand(
    Guid BookingId,
    DateTimeOffset ConfirmedAtUtc) : ICommand<BookingResponse>;
