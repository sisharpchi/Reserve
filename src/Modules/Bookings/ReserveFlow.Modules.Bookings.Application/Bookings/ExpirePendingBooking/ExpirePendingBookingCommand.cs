using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.ExpirePendingBooking;

public sealed record ExpirePendingBookingCommand(
    Guid BookingId,
    DateTimeOffset ExpiredAtUtc) : ICommand<BookingResponse?>;
