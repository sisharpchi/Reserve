using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Scheduling.Application.Availability.GetTenantAvailableSlots;

public sealed record GetTenantAvailableSlotsQuery(
    Guid TenantId,
    DateOnly Date,
    Guid? StaffMemberId,
    Guid? ResourceId,
    int DurationMinutes,
    int? StepMinutes) : IQuery<IReadOnlyList<AvailableSlotResponse>>;
