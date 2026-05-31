using ReserveFlow.Modules.Bookings.Domain.BookingHistory;

namespace ReserveFlow.Modules.Bookings.Application.BookingHistory;

public interface IBookingHistoryRepository
{
    void Insert(BookingHistoryEntry historyEntry);
}
