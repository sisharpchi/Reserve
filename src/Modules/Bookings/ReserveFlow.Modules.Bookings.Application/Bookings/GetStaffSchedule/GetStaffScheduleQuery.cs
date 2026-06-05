using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetStaffSchedule;

public sealed record GetStaffScheduleQuery(Guid TenantId, Guid StaffMemberId)
    : IQuery<IReadOnlyList<BookingResponse>>;
