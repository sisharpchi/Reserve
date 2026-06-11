using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources;

public interface IResourceRepository
{
    void Insert(Resource resource);

    Task<Resource?> GetByIdAsync(Guid resourceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Resource>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Resource>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        string? resourceType,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Resource>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
