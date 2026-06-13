using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources.GetResources;

public sealed class GetResourcesQueryHandler(IResourceRepository resourceRepository)
    : IQueryHandler<GetResourcesQuery, PagedResponse<ResourceResponse>>
{
    public async Task<PagedResponse<ResourceResponse>> Handle(
        GetResourcesQuery query,
        CancellationToken cancellationToken = default)
    {
        PageRequest pageRequest = PageRequest.Create(query.PageNumber, query.PageSize);
        PagedResult<Resource> resources = await resourceRepository.GetByTenantIdAsync(
            query.TenantId,
            pageRequest,
            query.Search,
            query.IsActive,
            query.ResourceType,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        ResourceResponse[] items = resources.Items
            .Select(ResourceResponse.FromResource)
            .ToArray();

        return new PagedResponse<ResourceResponse>(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            resources.TotalCount);
    }
}
