using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Tenants.Domain.Tenants;

public sealed record TenantSuspendedDomainEvent(
    Guid TenantId,
    DateTimeOffset SuspendedAtUtc) : DomainEvent;
