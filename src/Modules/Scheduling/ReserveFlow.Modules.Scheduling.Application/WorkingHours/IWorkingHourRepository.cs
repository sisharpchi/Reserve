using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;

namespace ReserveFlow.Modules.Scheduling.Application.WorkingHours;

public interface IWorkingHourRepository
{
    void Insert(WorkingHour workingHour);

    Task<IReadOnlyList<WorkingHour>> GetByTargetAndDayAsync(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DayOfWeek dayOfWeek,
        CancellationToken cancellationToken = default);
}
