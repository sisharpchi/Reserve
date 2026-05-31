using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Application.BookingPolicies;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.CancelBooking;

public sealed class CancelBookingCommandHandler(
    IBookingRepository bookingRepository,
    IBookingPolicyRepository bookingPolicyRepository,
    IBookingHistoryRepository bookingHistoryRepository,
    ITenantAccessGuard tenantAccessGuard,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<CancelBookingCommand, BookingResponse?>
{
    public async Task<BookingResponse?> Handle(
        CancelBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        Booking booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        if (command.EnforceTenantAccess && !tenantAccessGuard.CanAccessTenant(booking.TenantId))
        {
            return null;
        }

        if (command.EnforcePolicy)
        {
            BookingPolicy? policy = await bookingPolicyRepository.GetByTenantIdAsync(
                booking.TenantId,
                cancellationToken);

            policy?.EnsureCancellationAllowed(booking.StartsAtUtc, command.CancelledAtUtc);
        }

        booking.Cancel(command.CancelledAtUtc);
        bookingHistoryRepository.Insert(BookingHistoryEntry.Record(
            booking.TenantId,
            booking.Id,
            booking.Status,
            command.CancelledAtUtc,
            "Booking cancelled"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingResponse.FromBooking(booking);
    }
}
