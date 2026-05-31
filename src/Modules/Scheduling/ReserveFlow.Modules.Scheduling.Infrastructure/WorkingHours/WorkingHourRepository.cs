using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;
using ReserveFlow.Modules.Scheduling.Infrastructure.Database;

namespace ReserveFlow.Modules.Scheduling.Infrastructure.WorkingHours;

internal sealed class WorkingHourRepository(SchedulingDbContext dbContext) : IWorkingHourRepository
{
    public void Insert(WorkingHour workingHour)
    {
        dbContext.WorkingHours.Add(workingHour);
    }

    public async Task<IReadOnlyList<WorkingHour>> GetByTargetAndDayAsync(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DayOfWeek dayOfWeek,
        CancellationToken cancellationToken = default)
    {
        IQueryable<WorkingHour> query = dbContext.WorkingHours
            .Where(workingHour => workingHour.TenantId == tenantId &&
                                  workingHour.DayOfWeek == dayOfWeek);

        if (staffMemberId is not null && resourceId is not null)
        {
            query = query.Where(workingHour =>
                workingHour.StaffMemberId == staffMemberId.Value ||
                workingHour.ResourceId == resourceId.Value);
        }
        else if (staffMemberId is not null)
        {
            query = query.Where(workingHour => workingHour.StaffMemberId == staffMemberId.Value);
        }
        else if (resourceId is not null)
        {
            query = query.Where(workingHour => workingHour.ResourceId == resourceId.Value);
        }
        else
        {
            return [];
        }

        return await query
            .OrderBy(workingHour => workingHour.StartsAt)
            .ToArrayAsync(cancellationToken);
    }
}
