using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources.GetActiveResources;

public sealed class GetActiveResourcesQueryHandler(IResourceRepository resourceRepository)
    : IQueryHandler<GetActiveResourcesQuery, IReadOnlyList<ResourceResponse>>
{
    public async Task<IReadOnlyList<ResourceResponse>> Handle(
        GetActiveResourcesQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Resource> resources = await resourceRepository.GetActiveByTenantIdAsync(
            query.TenantId,
            cancellationToken);

        return resources
            .Select(ResourceResponse.FromResource)
            .ToArray();
    }
}
