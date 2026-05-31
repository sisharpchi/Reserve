using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.CompleteBooking;

public sealed record CompleteBookingCommand(
    Guid BookingId,
    DateTimeOffset CompletedAtUtc) : ICommand<BookingResponse?>;
