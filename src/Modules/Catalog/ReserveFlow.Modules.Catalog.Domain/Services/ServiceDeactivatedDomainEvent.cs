using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Catalog.Domain.Services;

public sealed record ServiceDeactivatedDomainEvent(
    Guid ServiceId,
    Guid TenantId) : DomainEvent;
