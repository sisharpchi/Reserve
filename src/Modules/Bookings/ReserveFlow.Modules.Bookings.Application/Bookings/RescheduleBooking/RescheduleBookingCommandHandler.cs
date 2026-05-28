using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.RescheduleBooking;

public sealed class RescheduleBookingCommandHandler(
    IBookingRepository bookingRepository,
    IBookingAvailabilityChecker availabilityChecker,
    IBookingHistoryRepository bookingHistoryRepository,
    ITenantAccessGuard tenantAccessGuard,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<RescheduleBookingCommand, BookingResponse?>
{
    public async Task<BookingResponse?> Handle(
        RescheduleBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        Booking booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        if (!tenantAccessGuard.CanAccessTenant(booking.TenantId))
        {
            return null;
        }

        bool isAvailable = await availabilityChecker.IsAvailableAsync(
            booking.TenantId,
            booking.StaffMemberId,
            booking.ResourceId,
            command.StartsAtUtc,
            command.EndsAtUtc,
            cancellationToken);

        if (!isAvailable)
        {
            throw new InvalidOperationException("Rescheduled booking is outside configured availability.");
        }

        bool hasOverlap = await bookingRepository.HasOverlapAsync(
            booking.TenantId,
            booking.StaffMemberId,
            booking.ResourceId,
            command.StartsAtUtc,
            command.EndsAtUtc,
            excludedBookingId: booking.Id,
            cancellationToken);

        if (hasOverlap)
        {
            throw new InvalidOperationException("Rescheduled booking overlaps an existing active booking.");
        }

        booking.Reschedule(command.StartsAtUtc, command.EndsAtUtc);
        bookingHistoryRepository.Insert(BookingHistoryEntry.Record(
            booking.TenantId,
            booking.Id,
            booking.Status,
            DateTimeOffset.UtcNow,
            "Booking rescheduled"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingResponse.FromBooking(booking);
    }
}
