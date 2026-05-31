using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;

namespace ReserveFlow.Modules.Bookings.Application.BookingPolicies;

public interface IBookingPolicyRepository
{
    void Insert(BookingPolicy bookingPolicy);

    Task<BookingPolicy?> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
