using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetBookings;

public sealed record GetBookingsQuery(Guid TenantId)
    : IQuery<IReadOnlyList<BookingResponse>>;
