using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPlatformTenantCategories;

public sealed class GetPlatformTenantCategoriesQueryHandler(ITenantCategoryRepository categoryRepository)
    : IQueryHandler<GetPlatformTenantCategoriesQuery, IReadOnlyList<TenantCategoryResponse>>
{
    public async Task<IReadOnlyList<TenantCategoryResponse>> Handle(
        GetPlatformTenantCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);

        return categories
            .Select(TenantCategoryResponse.FromCategory)
            .ToArray();
    }
}
