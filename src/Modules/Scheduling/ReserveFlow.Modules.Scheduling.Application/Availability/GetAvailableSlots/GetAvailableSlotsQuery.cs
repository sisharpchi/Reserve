using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Scheduling.Application.Availability;

namespace ReserveFlow.Modules.Scheduling.Application.Availability.GetAvailableSlots;

public sealed record GetAvailableSlotsQuery(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    int DurationMinutes,
    int StepMinutes) : IQuery<IReadOnlyList<AvailableSlotResponse>>;
