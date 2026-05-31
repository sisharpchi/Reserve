using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetPublicBooking;

public sealed record GetPublicBookingQuery(
    string PublicReference,
    string AccessToken) : IQuery<BookingResponse?>;
