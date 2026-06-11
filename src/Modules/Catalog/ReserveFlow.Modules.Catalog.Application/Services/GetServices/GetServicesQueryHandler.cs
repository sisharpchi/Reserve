using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetServices;

public sealed class GetServicesQueryHandler(IServiceRepository serviceRepository)
    : IQueryHandler<GetServicesQuery, PagedResponse<ServiceResponse>>
{
    public async Task<PagedResponse<ServiceResponse>> Handle(
        GetServicesQuery query,
        CancellationToken cancellationToken = default)
    {
        PageRequest pageRequest = PageRequest.Create(query.PageNumber, query.PageSize);
        PagedResult<Service> services = await serviceRepository.GetByTenantIdAsync(
            query.TenantId,
            pageRequest,
            query.Search,
            query.IsActive,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        ServiceResponse[] items = services.Items
            .Select(ServiceResponse.FromService)
            .ToArray();

        return new PagedResponse<ServiceResponse>(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            services.TotalCount);
    }
}
