using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetBookings;

public sealed class GetBookingsQueryHandler(IBookingRepository bookingRepository)
    : IQueryHandler<GetBookingsQuery, PagedResponse<BookingResponse>>
{
    public async Task<PagedResponse<BookingResponse>> Handle(
        GetBookingsQuery query,
        CancellationToken cancellationToken = default)
    {
        PageRequest pageRequest = PageRequest.Create(query.PageNumber, query.PageSize);
        PagedResult<Booking> bookings = await bookingRepository.GetByTenantIdAsync(
            query.TenantId,
            pageRequest,
            query.Status,
            query.FromUtc,
            query.ToUtc,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        BookingResponse[] items = bookings.Items
            .Select(BookingResponse.FromBooking)
            .ToArray();

        return new PagedResponse<BookingResponse>(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            bookings.TotalCount);
    }
}
