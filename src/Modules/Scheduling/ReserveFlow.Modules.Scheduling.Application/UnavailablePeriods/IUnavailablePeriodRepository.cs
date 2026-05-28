using ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;

namespace ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods;

public interface IUnavailablePeriodRepository
{
    void Insert(UnavailablePeriod unavailablePeriod);

    Task<IReadOnlyList<UnavailablePeriod>> GetByTargetAndRangeAsync(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset rangeStartsAtUtc,
        DateTimeOffset rangeEndsAtUtc,
        CancellationToken cancellationToken = default);
}
