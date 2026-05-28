using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.CancelBooking;

public sealed record CancelBookingCommand(
    Guid BookingId,
    DateTimeOffset CancelledAtUtc,
    bool EnforcePolicy = true,
    bool EnforceTenantAccess = true) : ICommand<BookingResponse?>;
