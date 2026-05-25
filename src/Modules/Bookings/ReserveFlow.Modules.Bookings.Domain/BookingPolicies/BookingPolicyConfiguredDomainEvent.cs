using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.BookingPolicies;

public sealed record BookingPolicyConfiguredDomainEvent(
    Guid BookingPolicyId,
    Guid TenantId,
    int MinimumAdvanceMinutes,
    int CancellationDeadlineHours) : DomainEvent;
