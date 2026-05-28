using Microsoft.EntityFrameworkCore;
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
        string publicReference,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicReference) ||
            string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        string normalizedPublicReference = Booking.NormalizePublicReference(publicReference);
        string normalizedAccessToken = accessToken.Trim();

        return await dbContext.Bookings.FirstOrDefaultAsync(
            booking => booking.PublicReference == normalizedPublicReference &&
                       booking.AccessToken == normalizedAccessToken,
            cancellationToken);
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
