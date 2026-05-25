using ReserveFlow.Modules.Scheduling.Domain.Availability;

namespace ReserveFlow.Modules.Scheduling.Application.Availability;

public sealed record AvailableSlotResponse(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc)
{
    public static AvailableSlotResponse FromSlot(AvailableSlot slot)
    {
        return new AvailableSlotResponse(slot.StartsAtUtc, slot.EndsAtUtc);
    }
}
