using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetPublicBooking;

public sealed record GetPublicBookingQuery(
    Guid TenantId,
    string PublicReference,
    string AccessToken) : IQuery<BookingResponse?>;
