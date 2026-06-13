using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Application.BookingPolicies;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.CancelPublicBooking;

public sealed class CancelPublicBookingCommandHandler(
    IBookingRepository bookingRepository,
    IBookingPolicyRepository bookingPolicyRepository,
    IBookingHistoryRepository bookingHistoryRepository,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<CancelPublicBookingCommand, BookingResponse?>
{
    public async Task<BookingResponse?> Handle(
        CancelPublicBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        Booking? booking = await bookingRepository.FindByPublicLookupAsync(
            command.TenantId,
            command.PublicReference,
            command.AccessToken,
            cancellationToken);

        if (booking is null)
        {
            return null;
        }

        BookingPolicy? policy = await bookingPolicyRepository.GetByTenantIdAsync(
            booking.TenantId,
            cancellationToken);

        policy?.EnsureCancellationAllowed(booking.StartsAtUtc, command.CancelledAtUtc);

        booking.Cancel(command.CancelledAtUtc);
        bookingHistoryRepository.Insert(BookingHistoryEntry.Record(
            booking.TenantId,
            booking.Id,
            booking.Status,
            command.CancelledAtUtc,
            "Public booking cancelled"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingResponse.FromBooking(booking);
    }
}
