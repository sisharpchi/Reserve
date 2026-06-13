using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Domain.Bookings;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Bookings;

internal sealed class BookingRepository(BookingsDbContext dbContext) : IBookingRepository
{
    public void Insert(Booking booking)
    {
        dbContext.Bookings.Add(booking);
    }

    public async Task<Booking?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .FirstOrDefaultAsync(booking => booking.Id == bookingId, cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .Where(booking => booking.TenantId == tenantId)
            .OrderByDescending(booking => booking.StartsAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResult<Booking>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? status,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Booking> query = dbContext.Bookings
            .Where(booking => booking.TenantId == tenantId);

        query = ApplyStatusFilter(query, status);
        query = ApplyDateRangeFilter(query, fromUtc, toUtc);

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        Booking[] items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<Booking>(items, totalCount);
    }

    public async Task<IReadOnlyList<Booking>> GetByTenantIdAndStaffMemberIdAsync(
        Guid tenantId,
        Guid staffMemberId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .Where(booking => booking.TenantId == tenantId &&
                              booking.StaffMemberId == staffMemberId)
            .OrderBy(booking => booking.StartsAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Booking?> GetByTenantIdAndIdAsync(
        Guid tenantId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings.FirstOrDefaultAsync(
            booking => booking.TenantId == tenantId && booking.Id == bookingId,
            cancellationToken);
    }

    public async Task<Booking?> FindByIdempotencyKeyAsync(
        Guid tenantId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        string? normalizedIdempotencyKey = Booking.NormalizeIdempotencyKey(idempotencyKey);

        if (normalizedIdempotencyKey is null)
        {
            return null;
        }

        return await dbContext.Bookings.FirstOrDefaultAsync(
            booking => booking.TenantId == tenantId && booking.IdempotencyKey == normalizedIdempotencyKey,
            cancellationToken);
    }

    public async Task<Booking?> FindByPublicLookupAsync(
        Guid tenantId,
        string publicReference,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty ||
            string.IsNullOrWhiteSpace(publicReference) ||
            string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        string normalizedPublicReference = Booking.NormalizePublicReference(publicReference);
        string normalizedAccessToken = accessToken.Trim();

        return await dbContext.Bookings.FirstOrDefaultAsync(
            booking => booking.TenantId == tenantId &&
                       booking.PublicReference == normalizedPublicReference &&
                       booking.AccessToken == normalizedAccessToken,
            cancellationToken);
    }

    private static IQueryable<Booking> ApplyStatusFilter(IQueryable<Booking> query, string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return query;
        }

        return Enum.TryParse(status.Trim(), ignoreCase: true, out BookingStatus bookingStatus)
            ? query.Where(booking => booking.Status == bookingStatus)
            : query;
    }

    private static IQueryable<Booking> ApplyDateRangeFilter(
        IQueryable<Booking> query,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc)
    {
        if (fromUtc.HasValue)
        {
            query = query.Where(booking => booking.StartsAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(booking => booking.StartsAtUtc <= toUtc.Value);
        }

        return query;
    }

    private static IOrderedQueryable<Booking> ApplySorting(
        IQueryable<Booking> query,
        string? sortBy,
        string? sortDirection)
    {
        bool descending = IsDescending(sortDirection, defaultValue: true);

        return NormalizeSortKey(sortBy) switch
        {
            "status" => descending
                ? query.OrderByDescending(booking => booking.Status).ThenByDescending(booking => booking.StartsAtUtc)
                : query.OrderBy(booking => booking.Status).ThenBy(booking => booking.StartsAtUtc),
            "endsat" or "endsatutc" => descending
                ? query.OrderByDescending(booking => booking.EndsAtUtc).ThenByDescending(booking => booking.StartsAtUtc)
                : query.OrderBy(booking => booking.EndsAtUtc).ThenBy(booking => booking.StartsAtUtc),
            _ => descending
                ? query.OrderByDescending(booking => booking.StartsAtUtc)
                : query.OrderBy(booking => booking.StartsAtUtc)
        };
    }

    private static bool IsDescending(string? sortDirection, bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
        {
            return defaultValue;
        }

        return string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sortDirection, "descending", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeSortKey(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? string.Empty
            : sortBy.Trim().Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    }

    public async Task<bool> HasOverlapAsync(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        Guid? excludedBookingId = null,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings.AnyAsync(
            booking =>
                booking.TenantId == tenantId &&
                (!excludedBookingId.HasValue || booking.Id != excludedBookingId.Value) &&
                (booking.Status == BookingStatus.Pending ||
                 booking.Status == BookingStatus.Confirmed ||
                 booking.Status == BookingStatus.Rescheduled) &&
                booking.StartsAtUtc < endsAtUtc &&
                startsAtUtc < booking.EndsAtUtc &&
                ((staffMemberId.HasValue && booking.StaffMemberId == staffMemberId) ||
                 (resourceId.HasValue && booking.ResourceId == resourceId)),
            cancellationToken);
    }
}
