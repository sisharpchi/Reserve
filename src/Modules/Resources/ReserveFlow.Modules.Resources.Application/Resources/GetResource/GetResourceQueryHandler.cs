using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources.GetResource;

public sealed class GetResourceQueryHandler(IResourceRepository resourceRepository)
    : IQueryHandler<GetResourceQuery, ResourceResponse?>
{
    public async Task<ResourceResponse?> Handle(
        GetResourceQuery query,
        CancellationToken cancellationToken = default)
    {
        Resource? resource = await resourceRepository.GetByIdAsync(query.ResourceId, cancellationToken);

        return resource is null ? null : ResourceResponse.FromResource(resource);
    }
}
