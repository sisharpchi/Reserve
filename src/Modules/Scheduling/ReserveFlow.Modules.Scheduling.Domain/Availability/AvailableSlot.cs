namespace ReserveFlow.Modules.Scheduling.Domain.Availability;

public sealed record AvailableSlot(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);
