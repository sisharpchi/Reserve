using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.CancelPublicBooking;

public sealed record CancelPublicBookingCommand(
    string PublicReference,
    string AccessToken,
    DateTimeOffset CancelledAtUtc) : ICommand<BookingResponse?>;
