using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;

namespace ReserveFlow.Modules.Scheduling.Application.WorkingHours;

public sealed record WorkingHourResponse(
    Guid Id,
    Guid TenantId,
    Guid? StaffMemberId,
    Guid? ResourceId,
    string DayOfWeek,
    TimeOnly StartsAt,
    TimeOnly EndsAt)
{
    public static WorkingHourResponse FromWorkingHour(WorkingHour workingHour)
    {
        return new WorkingHourResponse(
            workingHour.Id,
            workingHour.TenantId,
            workingHour.StaffMemberId,
            workingHour.ResourceId,
            workingHour.DayOfWeek.ToString(),
            workingHour.StartsAt,
            workingHour.EndsAt);
    }
}
