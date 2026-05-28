namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed record CreateUnavailablePeriodRequest(
    Guid TenantId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string? Reason);
