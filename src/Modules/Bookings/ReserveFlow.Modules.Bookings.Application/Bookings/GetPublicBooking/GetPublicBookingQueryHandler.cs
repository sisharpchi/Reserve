using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetPublicBooking;

public sealed class GetPublicBookingQueryHandler(IBookingRepository bookingRepository)
    : IQueryHandler<GetPublicBookingQuery, BookingResponse?>
{
    public async Task<BookingResponse?> Handle(
        GetPublicBookingQuery query,
        CancellationToken cancellationToken = default)
    {
        Booking? booking = await bookingRepository.FindByPublicLookupAsync(
            query.TenantId,
            query.PublicReference,
            query.AccessToken,
            cancellationToken);

        return booking is null ? null : BookingResponse.FromBooking(booking);
    }
}
