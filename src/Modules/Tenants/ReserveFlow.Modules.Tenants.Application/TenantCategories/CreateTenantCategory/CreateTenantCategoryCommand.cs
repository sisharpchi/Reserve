using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.CreateTenantCategory;

public sealed record CreateTenantCategoryCommand(
    string Name,
    string Slug,
    int SortOrder = 0) : ICommand<TenantCategoryResponse>;
