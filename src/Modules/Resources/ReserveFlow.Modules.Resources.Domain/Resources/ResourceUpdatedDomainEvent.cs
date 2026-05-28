using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Resources.Domain.Resources;

public sealed record ResourceUpdatedDomainEvent(
    Guid ResourceId,
    Guid TenantId,
    string Name,
    string ResourceType) : DomainEvent;
