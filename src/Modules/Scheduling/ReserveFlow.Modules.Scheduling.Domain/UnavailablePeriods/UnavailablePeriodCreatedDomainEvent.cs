using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;

public sealed record UnavailablePeriodCreatedDomainEvent(
    Guid UnavailablePeriodId,
    Guid TenantId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : DomainEvent;
