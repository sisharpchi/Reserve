using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;

namespace ReserveFlow.Modules.Bookings.Application.BookingPolicies;

public sealed record BookingPolicyResponse(
    Guid Id,
    Guid TenantId,
    int MinimumAdvanceMinutes,
    int CancellationDeadlineHours)
{
    public static BookingPolicyResponse FromPolicy(BookingPolicy policy)
    {
        return new BookingPolicyResponse(
            policy.Id,
            policy.TenantId,
            policy.MinimumAdvanceMinutes,
            policy.CancellationDeadlineHours);
    }
}
