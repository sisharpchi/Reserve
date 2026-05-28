using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Scheduling.Application.WorkingHours.CreateWorkingHour;

public sealed record CreateWorkingHourCommand(
    Guid TenantId,
    Guid? StaffMemberId,
    Guid? ResourceId,
    DayOfWeek DayOfWeek,
    TimeOnly StartsAt,
    TimeOnly EndsAt) : ICommand<WorkingHourResponse>;
