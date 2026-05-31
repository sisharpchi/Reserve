using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;

namespace ReserveFlow.Modules.Bookings.Infrastructure.BookingHistory;

internal sealed class BookingHistoryRepository(BookingsDbContext dbContext) : IBookingHistoryRepository
{
    public void Insert(BookingHistoryEntry historyEntry)
    {
        dbContext.BookingHistoryEntries.Add(historyEntry);
    }
}
