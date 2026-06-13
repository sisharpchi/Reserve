using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services;

public interface IServiceRepository
{
    void Insert(Service service);

    Task<Service?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<Service?> GetActiveByIdAsync(
        Guid tenantId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Service>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
