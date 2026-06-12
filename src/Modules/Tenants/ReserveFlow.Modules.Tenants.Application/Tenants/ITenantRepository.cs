using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants;

public interface ITenantRepository
{
    void Insert(Tenant tenant);

    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<Tenant>> GetAllAsync(
        PageRequest pageRequest,
        Guid? categoryId,
        string? search,
        string? status,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tenant>> GetPublicAsync(
        Guid? categoryId,
        string? search,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
