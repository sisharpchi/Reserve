using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetStaffSchedule;

public sealed class GetStaffScheduleQueryHandler(IBookingRepository bookingRepository)
    : IQueryHandler<GetStaffScheduleQuery, IReadOnlyList<BookingResponse>>
{
    public async Task<IReadOnlyList<BookingResponse>> Handle(
        GetStaffScheduleQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Booking> bookings = await bookingRepository.GetByTenantIdAndStaffMemberIdAsync(
            query.TenantId,
            query.StaffMemberId,
            cancellationToken);

        return bookings
            .Select(BookingResponse.FromBooking)
            .ToArray();
    }
}
