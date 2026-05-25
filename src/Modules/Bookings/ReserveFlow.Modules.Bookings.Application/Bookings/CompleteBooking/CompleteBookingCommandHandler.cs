using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.CompleteBooking;

public sealed class CompleteBookingCommandHandler(
    IBookingRepository bookingRepository,
    IBookingHistoryRepository bookingHistoryRepository,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<CompleteBookingCommand, BookingResponse>
{
    public async Task<BookingResponse> Handle(
        CompleteBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        Booking booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        booking.Complete(command.CompletedAtUtc);
        bookingHistoryRepository.Insert(BookingHistoryEntry.Record(
            booking.TenantId,
            booking.Id,
            booking.Status,
            command.CompletedAtUtc,
            "Booking completed"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingResponse.FromBooking(booking);
    }
}
