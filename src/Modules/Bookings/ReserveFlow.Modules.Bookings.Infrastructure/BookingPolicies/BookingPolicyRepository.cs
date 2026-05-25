using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Bookings.Application.BookingPolicies;
using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;

namespace ReserveFlow.Modules.Bookings.Infrastructure.BookingPolicies;

internal sealed class BookingPolicyRepository(BookingsDbContext dbContext) : IBookingPolicyRepository
{
    public void Insert(BookingPolicy bookingPolicy)
    {
        dbContext.BookingPolicies.Add(bookingPolicy);
    }

    public async Task<BookingPolicy?> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.BookingPolicies.FirstOrDefaultAsync(
            policy => policy.TenantId == tenantId,
            cancellationToken);
    }
}
