using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Infrastructure.Database;

namespace ReserveFlow.Modules.Scheduling.Infrastructure.UnavailablePeriods;

internal sealed class UnavailablePeriodRepository(SchedulingDbContext dbContext) : IUnavailablePeriodRepository
{
    public void Insert(UnavailablePeriod unavailablePeriod)
    {
        dbContext.UnavailablePeriods.Add(unavailablePeriod);
    }

    public async Task<IReadOnlyList<UnavailablePeriod>> GetByTargetAndRangeAsync(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset rangeStartsAtUtc,
        DateTimeOffset rangeEndsAtUtc,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UnavailablePeriod> query = dbContext.UnavailablePeriods
            .Where(unavailablePeriod => unavailablePeriod.TenantId == tenantId &&
                                        unavailablePeriod.StartsAtUtc < rangeEndsAtUtc &&
                                        rangeStartsAtUtc < unavailablePeriod.EndsAtUtc);

        if (staffMemberId is not null && resourceId is not null)
        {
            query = query.Where(unavailablePeriod =>
                unavailablePeriod.StaffMemberId == staffMemberId.Value ||
                unavailablePeriod.ResourceId == resourceId.Value);
        }
        else if (staffMemberId is not null)
        {
            query = query.Where(unavailablePeriod => unavailablePeriod.StaffMemberId == staffMemberId.Value);
        }
        else if (resourceId is not null)
        {
            query = query.Where(unavailablePeriod => unavailablePeriod.ResourceId == resourceId.Value);
        }
        else
        {
            return [];
        }

        return await query
            .OrderBy(unavailablePeriod => unavailablePeriod.StartsAtUtc)
            .ToArrayAsync(cancellationToken);
    }
}
