using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Catalog.Domain.Services;

public sealed record ServiceCreatedDomainEvent(
    Guid ServiceId,
    Guid TenantId,
    string Name,
    int DurationMinutes) : DomainEvent;
