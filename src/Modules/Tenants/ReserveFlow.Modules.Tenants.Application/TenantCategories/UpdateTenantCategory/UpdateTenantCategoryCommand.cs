using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.UpdateTenantCategory;

public sealed record UpdateTenantCategoryCommand(
    Guid CategoryId,
    string Name,
    string Slug,
    int SortOrder) : ICommand<TenantCategoryResponse?>;
