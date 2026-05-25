using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.ConfirmBooking;

public sealed class ConfirmBookingCommandHandler(
    IBookingRepository bookingRepository,
    IBookingHistoryRepository bookingHistoryRepository,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<ConfirmBookingCommand, BookingResponse>
{
    public async Task<BookingResponse> Handle(
        ConfirmBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        Booking booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        booking.Confirm(command.ConfirmedAtUtc);
        bookingHistoryRepository.Insert(BookingHistoryEntry.Record(
            booking.TenantId,
            booking.Id,
            booking.Status,
            command.ConfirmedAtUtc,
            "Booking confirmed"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingResponse.FromBooking(booking);
    }
}
