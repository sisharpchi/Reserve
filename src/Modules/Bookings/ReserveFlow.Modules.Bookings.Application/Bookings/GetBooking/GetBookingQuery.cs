using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetBooking;

public sealed record GetBookingQuery(
    Guid TenantId,
    Guid BookingId) : IQuery<BookingResponse?>;
