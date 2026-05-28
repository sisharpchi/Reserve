using ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;

namespace ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods;

public sealed record UnavailablePeriodResponse(
    Guid Id,
    Guid TenantId,
    Guid? StaffMemberId,
    Guid? ResourceId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Reason)
{
    public static UnavailablePeriodResponse FromUnavailablePeriod(UnavailablePeriod unavailablePeriod)
    {
        return new UnavailablePeriodResponse(
            unavailablePeriod.Id,
            unavailablePeriod.TenantId,
            unavailablePeriod.StaffMemberId,
            unavailablePeriod.ResourceId,
            unavailablePeriod.StartsAtUtc,
            unavailablePeriod.EndsAtUtc,
            unavailablePeriod.Reason);
    }
}
