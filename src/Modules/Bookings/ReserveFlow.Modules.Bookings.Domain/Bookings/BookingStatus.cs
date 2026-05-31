namespace ReserveFlow.Modules.Bookings.Domain.Bookings;

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Rescheduled = 3,
    Completed = 4,
    NoShow = 5,
    Expired = 6
}
