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
}
