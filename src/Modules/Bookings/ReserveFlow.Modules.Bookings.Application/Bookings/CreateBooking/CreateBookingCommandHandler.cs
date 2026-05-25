using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Bookings.Application.BookingPolicies;
using ReserveFlow.Modules.Bookings.Application.BookingHistory;
using ReserveFlow.Modules.Bookings.Application.Customers;
using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Domain.Bookings;
using ReserveFlow.Modules.Bookings.Domain.Customers;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.CreateBooking;

public sealed class CreateBookingCommandHandler(
    IBookingRepository bookingRepository,
    IBookingPolicyRepository bookingPolicyRepository,
    IBookingHistoryRepository bookingHistoryRepository,
    ICustomerRepository customerRepository,
    IBookingsUnitOfWork unitOfWork) : ICommandHandler<CreateBookingCommand, BookingResponse>
{
    public async Task<BookingResponse> Handle(
        CreateBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            Booking? existingBooking = await bookingRepository.FindByIdempotencyKeyAsync(
                command.TenantId,
                command.IdempotencyKey,
                cancellationToken);

            if (existingBooking is not null)
            {
                return BookingResponse.FromBooking(existingBooking);
            }
        }

        BookingPolicy? policy = await bookingPolicyRepository.GetByTenantIdAsync(
            command.TenantId,
            cancellationToken);

        policy?.EnsureBookingCanStartAt(command.StartsAtUtc, DateTimeOffset.UtcNow);

        if (command.StaffMemberId is null && command.ResourceId is null)
        {
            throw new InvalidOperationException("Booking must target a staff member or resource.");
        }

        bool hasOverlap = await bookingRepository.HasOverlapAsync(
            command.TenantId,
            command.StaffMemberId,
            command.ResourceId,
            command.StartsAtUtc,
            command.EndsAtUtc,
            excludedBookingId: null,
            cancellationToken);

        if (hasOverlap)
        {
            throw new InvalidOperationException("Booking overlaps an existing active booking.");
        }

        Customer customer = await customerRepository.FindByEmailAsync(
            command.TenantId,
            command.CustomerEmail,
            cancellationToken) ?? Customer.Register(
                command.TenantId,
                command.CustomerName,
                command.CustomerEmail,
                command.CustomerPhoneNumber);

        if (customer.DomainEvents.Count > 0)
        {
            customerRepository.Insert(customer);
        }

        Booking booking = Booking.Create(
            command.TenantId,
            customer.Id,
            command.ServiceId,
            command.StaffMemberId,
            command.ResourceId,
            command.StartsAtUtc,
            command.EndsAtUtc,
            command.IdempotencyKey);

        bookingRepository.Insert(booking);
        bookingHistoryRepository.Insert(BookingHistoryEntry.Record(
            booking.TenantId,
            booking.Id,
            booking.Status,
            DateTimeOffset.UtcNow,
            "Booking created"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookingResponse.FromBooking(booking);
    }
}
