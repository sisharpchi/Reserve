using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetBooking;

public sealed class GetBookingQueryHandler(IBookingRepository bookingRepository)
    : IQueryHandler<GetBookingQuery, BookingResponse?>
{
    public async Task<BookingResponse?> Handle(
        GetBookingQuery query,
        CancellationToken cancellationToken = default)
    {
        Booking? booking = await bookingRepository.GetByTenantIdAndIdAsync(
            query.TenantId,
            query.BookingId,
            cancellationToken);

        return booking is null ? null : BookingResponse.FromBooking(booking);
    }
}
