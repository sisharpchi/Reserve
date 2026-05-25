using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings;

public interface IBookingRepository
{
    void Insert(Booking booking);

    Task<Booking?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<Booking?> FindByIdempotencyKeyAsync(
        Guid tenantId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlapAsync(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        Guid? excludedBookingId = null,
        CancellationToken cancellationToken = default);
}
