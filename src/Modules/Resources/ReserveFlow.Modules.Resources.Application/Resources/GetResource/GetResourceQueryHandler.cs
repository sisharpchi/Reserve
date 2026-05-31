using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources.GetResource;

public sealed class GetResourceQueryHandler(
    IResourceRepository resourceRepository,
    ITenantAccessGuard tenantAccessGuard)
    : IQueryHandler<GetResourceQuery, ResourceResponse?>
{
    public async Task<ResourceResponse?> Handle(
        GetResourceQuery query,
        CancellationToken cancellationToken = default)
    {
        Resource? resource = await resourceRepository.GetByIdAsync(query.ResourceId, cancellationToken);

        if (resource is null || !tenantAccessGuard.CanAccessTenant(resource.TenantId))
        {
            return null;
        }

        return ResourceResponse.FromResource(resource);
    }
}
