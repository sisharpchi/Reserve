using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetBookings;

public sealed class GetBookingsQueryHandler(IBookingRepository bookingRepository)
    : IQueryHandler<GetBookingsQuery, IReadOnlyList<BookingResponse>>
{
    public async Task<IReadOnlyList<BookingResponse>> Handle(
        GetBookingsQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Booking> bookings = await bookingRepository.GetByTenantIdAsync(
            query.TenantId,
            cancellationToken);

        return bookings
            .Select(BookingResponse.FromBooking)
            .ToArray();
    }
}
