using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services;

public interface IServiceRepository
{
    void Insert(Service service);

    Task<Service?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
