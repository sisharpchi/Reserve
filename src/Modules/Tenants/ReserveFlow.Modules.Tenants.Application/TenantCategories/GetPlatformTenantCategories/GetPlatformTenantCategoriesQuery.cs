using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPlatformTenantCategories;

public sealed record GetPlatformTenantCategoriesQuery(
    int? PageNumber,
    int? PageSize,
    string? Search,
    bool? IsActive,
    string? SortBy,
    string? SortDirection) : IQuery<PagedResponse<TenantCategoryResponse>>;
