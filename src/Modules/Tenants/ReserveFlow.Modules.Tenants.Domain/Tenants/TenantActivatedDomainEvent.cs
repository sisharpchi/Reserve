using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Tenants.Domain.Tenants;

public sealed record TenantActivatedDomainEvent(
    Guid TenantId,
    DateTimeOffset ActivatedAtUtc) : DomainEvent;
