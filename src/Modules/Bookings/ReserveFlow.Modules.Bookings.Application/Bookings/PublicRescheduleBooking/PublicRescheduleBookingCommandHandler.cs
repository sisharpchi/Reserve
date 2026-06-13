using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Application.BookingPolicies;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.PublicRescheduleBooking;

public sealed class PublicRescheduleBookingCommandHandler(
    IBookingRepository bookingRepository,
    IBookingPolicyRepository bookingPolicyRepository,
    IBookingAvailabilityChecker availabilityChecker,
    IBookingHistoryRepository bookingHistoryRepository,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<PublicRescheduleBookingCommand, BookingResponse?>
{
    public async Task<BookingResponse?> Handle(
        PublicRescheduleBookingCommand command,
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

        policy?.EnsureBookingCanStartAt(command.StartsAtUtc, command.RescheduledAtUtc);

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
            command.RescheduledAtUtc,
            "Public booking rescheduled"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingResponse.FromBooking(booking);
    }
}
