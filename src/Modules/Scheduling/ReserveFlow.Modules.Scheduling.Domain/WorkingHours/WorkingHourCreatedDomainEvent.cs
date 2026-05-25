using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Scheduling.Domain.WorkingHours;

public sealed record WorkingHourCreatedDomainEvent(
    Guid WorkingHourId,
    Guid TenantId,
    DayOfWeek DayOfWeek) : DomainEvent;
