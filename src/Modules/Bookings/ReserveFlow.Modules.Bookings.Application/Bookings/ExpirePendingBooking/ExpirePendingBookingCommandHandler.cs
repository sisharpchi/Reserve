using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.ExpirePendingBooking;

public sealed class ExpirePendingBookingCommandHandler(
    IBookingRepository bookingRepository,
    IBookingHistoryRepository bookingHistoryRepository,
    ITenantAccessGuard tenantAccessGuard,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<ExpirePendingBookingCommand, BookingResponse?>
{
    public async Task<BookingResponse?> Handle(
        ExpirePendingBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        Booking booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        if (!tenantAccessGuard.CanAccessTenant(booking.TenantId))
        {
            return null;
        }

        booking.Expire(command.ExpiredAtUtc);
        bookingHistoryRepository.Insert(BookingHistoryEntry.Record(
            booking.TenantId,
            booking.Id,
            booking.Status,
            command.ExpiredAtUtc,
            "Booking expired"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingResponse.FromBooking(booking);
    }
}
