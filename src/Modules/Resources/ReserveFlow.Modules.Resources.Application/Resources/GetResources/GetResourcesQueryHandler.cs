using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Resources.Application.Resources.GetResources;

public sealed class GetResourcesQueryHandler(IResourceRepository resourceRepository)
    : IQueryHandler<GetResourcesQuery, IReadOnlyList<ResourceResponse>>
{
    public async Task<IReadOnlyList<ResourceResponse>> Handle(
        GetResourcesQuery query,
        CancellationToken cancellationToken = default)
    {
        var resources = await resourceRepository.GetByTenantIdAsync(query.TenantId, cancellationToken);

        return resources
            .Select(ResourceResponse.FromResource)
            .ToArray();
    }
}
