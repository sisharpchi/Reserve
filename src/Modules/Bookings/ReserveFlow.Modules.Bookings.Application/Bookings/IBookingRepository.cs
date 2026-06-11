using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.Modules.Bookings.Application.Bookings;

public interface IBookingRepository
{
    void Insert(Booking booking);

    Task<Booking?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Booking>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? status,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetByTenantIdAndStaffMemberIdAsync(
        Guid tenantId,
        Guid staffMemberId,
        CancellationToken cancellationToken = default);

    Task<Booking?> GetByTenantIdAndIdAsync(
        Guid tenantId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<Booking?> FindByIdempotencyKeyAsync(
        Guid tenantId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<Booking?> FindByPublicLookupAsync(
        Guid tenantId,
        string publicReference,
        string accessToken,
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
