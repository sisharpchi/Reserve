using ReserveFlow.Modules.Tenants.Domain.TenantCategories;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories;

public sealed record TenantCategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    int SortOrder,
    bool IsActive)
{
    public static TenantCategoryResponse FromCategory(TenantCategory category)
    {
        return new TenantCategoryResponse(
            category.Id,
            category.Name,
            category.Slug,
            category.SortOrder,
            category.IsActive);
    }
}
