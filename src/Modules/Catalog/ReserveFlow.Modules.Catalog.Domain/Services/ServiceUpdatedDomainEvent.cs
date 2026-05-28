using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Catalog.Domain.Services;

public sealed record ServiceUpdatedDomainEvent(
    Guid ServiceId,
    Guid TenantId,
    string Name,
    int DurationMinutes) : DomainEvent;
