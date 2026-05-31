namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed record CreateWorkingHourRequest(
    Guid TenantId,
    DayOfWeek DayOfWeek,
    TimeOnly StartsAt,
    TimeOnly EndsAt);
