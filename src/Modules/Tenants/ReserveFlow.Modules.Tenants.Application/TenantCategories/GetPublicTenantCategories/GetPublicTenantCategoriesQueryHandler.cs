using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPublicTenantCategories;

public sealed class GetPublicTenantCategoriesQueryHandler(ITenantCategoryRepository categoryRepository)
    : IQueryHandler<GetPublicTenantCategoriesQuery, IReadOnlyList<TenantCategoryResponse>>
{
    public async Task<IReadOnlyList<TenantCategoryResponse>> Handle(
        GetPublicTenantCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TenantCategory> categories = await categoryRepository.GetActiveAsync(cancellationToken);

        return categories
            .Select(TenantCategoryResponse.FromCategory)
            .ToArray();
    }
}
