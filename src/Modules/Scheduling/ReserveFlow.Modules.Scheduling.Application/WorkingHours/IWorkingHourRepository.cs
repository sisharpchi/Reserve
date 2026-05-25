using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;

namespace ReserveFlow.Modules.Scheduling.Application.WorkingHours;

public interface IWorkingHourRepository
{
    void Insert(WorkingHour workingHour);
}
