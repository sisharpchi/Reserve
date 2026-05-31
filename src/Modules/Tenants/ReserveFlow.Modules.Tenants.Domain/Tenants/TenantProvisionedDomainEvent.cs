using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Tenants.Domain.Tenants;

public sealed record TenantProvisionedDomainEvent(
    Guid TenantId,
    string Slug,
    string TimeZoneId,
    Guid? CategoryId) : DomainEvent;
