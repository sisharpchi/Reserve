using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources;

public interface IResourceRepository
{
    void Insert(Resource resource);

    Task<IReadOnlyList<Resource>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
