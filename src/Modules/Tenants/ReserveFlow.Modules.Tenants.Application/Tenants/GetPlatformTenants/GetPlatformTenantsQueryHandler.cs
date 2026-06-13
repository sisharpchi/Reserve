using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformTenants;

public sealed class GetPlatformTenantsQueryHandler(ITenantRepository tenantRepository)
    : IQueryHandler<GetPlatformTenantsQuery, PagedResponse<TenantResponse>>
{
    public async Task<PagedResponse<TenantResponse>> Handle(
        GetPlatformTenantsQuery query,
        CancellationToken cancellationToken = default)
    {
        PageRequest pageRequest = PageRequest.Create(query.PageNumber, query.PageSize);
        PagedResult<Tenant> tenants = await tenantRepository.GetAllAsync(
            pageRequest,
            query.CategoryId,
            query.Search,
            query.Status,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        TenantResponse[] items = tenants.Items
            .Select(TenantResponse.FromTenant)
            .ToArray();

        return new PagedResponse<TenantResponse>(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            tenants.TotalCount);
    }
}
