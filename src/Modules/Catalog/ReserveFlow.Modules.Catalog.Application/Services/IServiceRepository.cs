using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services;

public interface IServiceRepository
{
    void Insert(Service service);

    Task<IReadOnlyList<Service>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
