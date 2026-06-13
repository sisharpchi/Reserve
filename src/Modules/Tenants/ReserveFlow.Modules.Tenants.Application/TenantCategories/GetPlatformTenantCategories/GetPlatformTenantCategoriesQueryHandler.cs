using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPlatformTenantCategories;

public sealed class GetPlatformTenantCategoriesQueryHandler(ITenantCategoryRepository categoryRepository)
    : IQueryHandler<GetPlatformTenantCategoriesQuery, PagedResponse<TenantCategoryResponse>>
{
    public async Task<PagedResponse<TenantCategoryResponse>> Handle(
        GetPlatformTenantCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        PageRequest pageRequest = PageRequest.Create(query.PageNumber, query.PageSize);
        PagedResult<TenantCategory> categories = await categoryRepository.GetAllAsync(
            pageRequest,
            query.Search,
            query.IsActive,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        TenantCategoryResponse[] items = categories.Items
            .Select(TenantCategoryResponse.FromCategory)
            .ToArray();

        return new PagedResponse<TenantCategoryResponse>(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            categories.TotalCount);
    }
}
