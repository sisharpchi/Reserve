using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Resources.Domain.Resources;

public sealed record ResourceDeactivatedDomainEvent(
    Guid ResourceId,
    Guid TenantId) : DomainEvent;
