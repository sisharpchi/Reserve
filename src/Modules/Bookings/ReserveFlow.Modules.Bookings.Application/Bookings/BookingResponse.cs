using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings;

public sealed record BookingResponse(
    Guid Id,
    Guid TenantId,
    Guid CustomerId,
    Guid ServiceId,
    Guid? StaffMemberId,
    Guid? ResourceId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string Status,
    Guid ConcurrencyToken)
{
    public static BookingResponse FromBooking(Booking booking)
    {
        return new BookingResponse(
            booking.Id,
            booking.TenantId,
            booking.CustomerId,
            booking.ServiceId,
            booking.StaffMemberId,
            booking.ResourceId,
            booking.StartsAtUtc,
            booking.EndsAtUtc,
            booking.Status.ToString(),
            booking.ConcurrencyToken);
    }
}
