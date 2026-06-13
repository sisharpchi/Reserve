using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Tenants.Domain.TenantCategories;

public sealed record TenantCategoryUpdatedDomainEvent(
    Guid CategoryId,
    string Slug,
    string Name) : DomainEvent;
