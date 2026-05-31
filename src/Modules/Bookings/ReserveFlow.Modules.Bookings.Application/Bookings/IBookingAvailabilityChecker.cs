namespace ReserveFlow.Modules.Bookings.Application.Bookings;

public interface IBookingAvailabilityChecker
{
    Task<bool> IsAvailableAsync(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken = default);
}
