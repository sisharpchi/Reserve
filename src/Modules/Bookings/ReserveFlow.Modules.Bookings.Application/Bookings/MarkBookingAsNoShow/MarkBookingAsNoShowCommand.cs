using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.MarkBookingAsNoShow;

public sealed record MarkBookingAsNoShowCommand(
    Guid BookingId,
    DateTimeOffset MarkedAtUtc) : ICommand<BookingResponse?>;
