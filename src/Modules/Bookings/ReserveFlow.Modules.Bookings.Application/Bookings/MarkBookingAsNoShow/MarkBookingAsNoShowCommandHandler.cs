using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.MarkBookingAsNoShow;

public sealed class MarkBookingAsNoShowCommandHandler(
    IBookingRepository bookingRepository,
    IBookingHistoryRepository bookingHistoryRepository,
    ITenantAccessGuard tenantAccessGuard,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<MarkBookingAsNoShowCommand, BookingResponse?>
{
    public async Task<BookingResponse?> Handle(
        MarkBookingAsNoShowCommand command,
        CancellationToken cancellationToken = default)
    {
        Booking booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        if (!tenantAccessGuard.CanAccessTenant(booking.TenantId))
        {
            return null;
        }

        booking.MarkAsNoShow(command.MarkedAtUtc);
        bookingHistoryRepository.Insert(BookingHistoryEntry.Record(
            booking.TenantId,
            booking.Id,
            booking.Status,
            command.MarkedAtUtc,
            "Booking marked as no-show"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingResponse.FromBooking(booking);
    }
}
