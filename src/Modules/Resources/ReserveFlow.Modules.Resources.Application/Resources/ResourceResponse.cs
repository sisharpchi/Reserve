using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources;

public sealed record ResourceResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string ResourceType,
    int Capacity,
    bool IsActive)
{
    public static ResourceResponse FromResource(Resource resource)
    {
        return new ResourceResponse(
            resource.Id,
            resource.TenantId,
            resource.Name,
            resource.ResourceType,
            resource.Capacity,
            resource.IsActive);
    }
}
