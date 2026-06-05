namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed record CreateOwnUnavailablePeriodRequest(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Reason);
